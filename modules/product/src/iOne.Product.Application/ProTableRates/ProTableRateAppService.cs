using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using iOne.Product;
using iOne.Product.Localization;
using iOne.Product.Permissions;
using iOne.Product.ProTableRates;
using iOne.Product.ProTableRateVariables;
using iOne.ProAttributes;
using iOne.ProProducts;
using iOne.ProTableRateVariables;
using iOne.ProTableRates;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;

namespace iOne.Product.ProTableRates;

[RemoteService(Name = ProductRemoteServiceConsts.RemoteServiceName)]
[Authorize(ProTableRatePermissions.Default)]
public class ProTableRateAppService : CrudAppService<
    ProTableRate,
    ProTableRateDto,
    Guid,
    GetProTableRatesInput,
    CreateProTableRateDto,
    UpdateProTableRateDto>, IProTableRateAppService
{
    protected ProTableRateManager Manager { get; }
    protected IRepository<ProAttribute, Guid> AttributeRepository { get; }
    protected IRepository<ProProductTableRate, Guid> ProductTableRateRepository { get; }
    protected IIdentityUserRepository UserRepository { get; }

    public ProTableRateAppService(
        IProTableRateRepository repository,
        ProTableRateManager manager,
        IRepository<ProAttribute, Guid> attributeRepository,
        IRepository<ProProductTableRate, Guid> productTableRateRepository,
        IIdentityUserRepository userRepository)
        : base(repository)
    {
        Manager = manager;
        AttributeRepository = attributeRepository;
        ProductTableRateRepository = productTableRateRepository;
        UserRepository = userRepository;
        LocalizationResource = typeof(ProductResource);
        GetPolicyName = ProTableRatePermissions.View;
        GetListPolicyName = ProTableRatePermissions.View;
        CreatePolicyName = ProTableRatePermissions.Create;
        UpdatePolicyName = ProTableRatePermissions.Edit;
        DeletePolicyName = ProTableRatePermissions.Delete;
    }

    public override async Task<ProTableRateDto> CreateAsync(CreateProTableRateDto input)
    {
        var entity = new ProTableRate(
            GuidGenerator.Create(),
            input.LobId,
            input.Code,
            input.Name,
            input.Status,
            input.InsurerId,
            input.Description
        );

        await Manager.CreateAsync(entity);

        // Add Variables if provided
        if (input.Variables != null && input.Variables.Any())
        {
            foreach (var variableDto in input.Variables)
            {
                // Validate AttributeId
                var attribute = await AttributeRepository.GetAsync(variableDto.AttributeId);

                var variable = new ProTableRateVariable(
                    GuidGenerator.Create(),
                    entity.Id,
                    variableDto.AttributeId,
                    variableDto.Operator
                );

                entity.AddVariable(variable);
            }
        }

        await CurrentUnitOfWork.SaveChangesAsync();

        return await MapToDtoWithVariablesAsync(entity);
    }

    public override async Task<ProTableRateDto> UpdateAsync(Guid id, UpdateProTableRateDto input)
    {
        // Load entity with Variables using Include
        var query = await Repository.GetQueryableAsync();
        var entity = await query
            .Include(x => x.Variables)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity == null)
        {
            throw new EntityNotFoundException(typeof(ProTableRate), id);
        }

        await Manager.UpdateAsync(entity, input.Name, input.Status, input.LobId, input.InsurerId, input.Description);

        // Update Variables - remove all existing and add new ones
        if (input.Variables != null)
        {
            // Remove all existing variables
            var existingVariables = entity.Variables.ToList();
            foreach (var variable in existingVariables)
            {
                entity.RemoveVariable(variable);
            }

            // Add new variables
            foreach (var variableDto in input.Variables)
            {
                // Validate AttributeId
                var attribute = await AttributeRepository.GetAsync(variableDto.AttributeId);

                var variable = new ProTableRateVariable(
                    GuidGenerator.Create(),
                    entity.Id,
                    variableDto.AttributeId,
                    variableDto.Operator
                );

                entity.AddVariable(variable);
            }
        }

        await CurrentUnitOfWork.SaveChangesAsync();

        return await MapToDtoWithVariablesAsync(entity);
    }

    public override async Task DeleteAsync(Guid id)
    {
        // Load entity with Variables
        var query = await Repository.GetQueryableAsync();
        var entity = await query
            .Include(x => x.Variables)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity == null)
        {
            throw new EntityNotFoundException(typeof(ProTableRate), id);
        }

        // Delete to trigger audit logging with ChangeType = Deleted
        // FullAuditedAggregateRoot uses soft delete, so entity is not actually removed from database
        // Variables will be cascade deleted or handled by EF Core
        await Repository.DeleteAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();

        // Update status to Deactive (entity still exists in database due to soft delete)
        entity.UpdateStatus(ProTableRateStatus.Deactive);
        await Repository.UpdateAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    public override async Task<ProTableRateDto> GetAsync(Guid id)
    {
        // Load entity with Variables using Include
        var query = await Repository.GetQueryableAsync();
        var entity = await query
            .Include(x => x.Variables)
                .ThenInclude(v => v.Attribute)
            .Include(x => x.Lob)
            .Include(x => x.Insurer)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity == null)
        {
            throw new EntityNotFoundException(typeof(ProTableRate), id);
        }

        var dto = await MapToDtoWithVariablesAsync(entity);

        // Get CreatorName for each Attribute
        if (dto.Variables != null && dto.Variables.Any())
        {
            var creatorIds = entity.Variables
                .Where(v => v.Attribute?.CreatorId != null)
                .Select(v => v.Attribute!.CreatorId!.Value)
                .Distinct()
                .ToList();

            if (creatorIds.Count > 0)
            {
                var users = await UserRepository.GetListByIdsAsync(creatorIds);
                var userDictionary = users.ToDictionary(u => u.Id, u => u.Name ?? u.UserName);

                foreach (var variableDto in dto.Variables)
                {
                    var variable = entity.Variables.FirstOrDefault(v => v.Id == variableDto.Id);
                    if (variable?.Attribute != null)
                    {
                        variableDto.AttributeName = variable.Attribute.Name;
                        if (variable.Attribute.CreatorId.HasValue && 
                            userDictionary.TryGetValue(variable.Attribute.CreatorId.Value, out var creatorName))
                        {
                            variableDto.AttributeCreatorName = creatorName;
                        }
                    }
                }
            }
        }

        return dto;
    }

    protected override async Task<ProTableRateDto> MapToGetOutputDtoAsync(ProTableRate entity)
    {
        return await MapToDtoWithVariablesAsync(entity);
    }

    protected override async Task<ProTableRateDto> MapToGetListOutputDtoAsync(ProTableRate entity)
    {
        // For list, we may not have Variables loaded, so check if they exist
        var dto = ObjectMapper.Map<ProTableRate, ProTableRateDto>(entity);
        
        // Only map Variables if they are loaded (to avoid lazy loading issues)
        if (entity.Variables != null && entity.Variables.Any())
        {
            dto.Variables = ObjectMapper.Map<List<ProTableRateVariable>, List<ProTableRateVariableDto>>(entity.Variables.ToList());

            // Get CreatorName for each Attribute
            var creatorIds = entity.Variables
                .Where(v => v.Attribute?.CreatorId != null)
                .Select(v => v.Attribute!.CreatorId!.Value)
                .Distinct()
                .ToList();

            if (creatorIds.Count > 0)
            {
                var users = await UserRepository.GetListByIdsAsync(creatorIds);
                var userDictionary = users.ToDictionary(u => u.Id, u => u.Name ?? u.UserName);

                foreach (var variableDto in dto.Variables)
                {
                    var variable = entity.Variables.FirstOrDefault(v => v.Id == variableDto.Id);
                    if (variable?.Attribute != null)
                    {
                        variableDto.AttributeName = variable.Attribute.Name;
                        if (variable.Attribute.CreatorId.HasValue && 
                            userDictionary.TryGetValue(variable.Attribute.CreatorId.Value, out var creatorName))
                        {
                            variableDto.AttributeCreatorName = creatorName;
                        }
                    }
                }
            }
        }
        
        return dto;
    }

    private async Task<ProTableRateDto> MapToDtoWithVariablesAsync(ProTableRate entity)
    {
        var dto = ObjectMapper.Map<ProTableRate, ProTableRateDto>(entity);

        // Map Variables if loaded
        if (entity.Variables != null && entity.Variables.Any())
        {
            dto.Variables = ObjectMapper.Map<List<ProTableRateVariable>, List<ProTableRateVariableDto>>(entity.Variables.ToList());
        }

        return dto;
    }

    protected override async Task<IQueryable<ProTableRate>> CreateFilteredQueryAsync(GetProTableRatesInput input)
    {
        var query = await ReadOnlyRepository.GetQueryableAsync();

        // Eager load Variables collection and Attribute for list queries
        query = query
            .Include(x => x.Variables)
                .ThenInclude(v => v.Attribute);

        // Filter by Code (case-insensitive, contains)
        if (!string.IsNullOrWhiteSpace(input.Code))
        {
            query = query.Where(x => EF.Functions.ILike(x.Code, $"%{input.Code}%"));
        }

        // Filter by Name (case-insensitive, contains)
        if (!string.IsNullOrWhiteSpace(input.Name))
        {
            query = query.Where(x => EF.Functions.ILike(x.Name, $"%{input.Name}%"));
        }

        // Filter by Status
        if (input.Status.HasValue)
        {
            query = query.Where(x => x.Status == input.Status.Value);
        }

        // Filter by LobId
        if (input.LobId.HasValue)
        {
            query = query.Where(x => x.LobId == input.LobId.Value);
        }

        // Filter by InsurerId
        if (input.InsurerId.HasValue)
        {
            query = query.Where(x => x.InsurerId == input.InsurerId.Value);
        }

        // Filter by ProductId (via ProProductTableRate join table)
        if (input.ProductId.HasValue)
        {
            var productTableRateQuery = await ProductTableRateRepository.GetQueryableAsync();
            query = query.Where(x => productTableRateQuery
                .Any(ptr => ptr.TableRateId == x.Id && ptr.ProductId == input.ProductId.Value));
        }

        return query;
    }
}
