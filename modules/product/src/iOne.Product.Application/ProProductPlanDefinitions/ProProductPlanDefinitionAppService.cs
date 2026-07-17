using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using iOne.Product;
using iOne.Product.ProProductPlanDefinitions;
using iOne.Product.Localization;
using iOne.Product.Permissions;
using iOne.ProProductPlanDefinitions; // For Entity, Manager, Repository
using iOne.ProProducts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;

namespace iOne.Product.ProProductPlanDefinitions;

[RemoteService(Name = ProductRemoteServiceConsts.RemoteServiceName)]
[Authorize(ProProductPermissions.Default)]
public class ProProductPlanDefinitionAppService : CrudAppService<
    ProProductPlanDefinition,
    ProProductPlanDefinitionDto,
    Guid,
    GetProProductPlanDefinitionsInput,
    CreateProProductPlanDefinitionDto,
    UpdateProProductPlanDefinitionDto>, IProProductPlanDefinitionAppService
{
    protected ProProductPlanDefinitionManager Manager { get; }
    protected IProProductPlanDefinitionRepository PlanDefinitionRepository { get; }
    protected IProProductRepository ProductRepository { get; }
    protected IRepository<IdentityUser, Guid> UserRepository { get; }

    public ProProductPlanDefinitionAppService(
        IProProductPlanDefinitionRepository repository,
        ProProductPlanDefinitionManager manager,
        IProProductRepository productRepository,
        IRepository<IdentityUser, Guid> userRepository)
        : base(repository)
    {
        Manager = manager;
        PlanDefinitionRepository = repository;
        ProductRepository = productRepository;
        UserRepository = userRepository;
        LocalizationResource = typeof(ProductResource);
        GetPolicyName = ProProductPermissions.View;
        GetListPolicyName = ProProductPermissions.View;
        CreatePolicyName = ProProductPermissions.Create;
        UpdatePolicyName = ProProductPermissions.Edit;
        DeletePolicyName = ProProductPermissions.Delete;
    }

    public override async Task<ProProductPlanDefinitionDto> CreateAsync(CreateProProductPlanDefinitionDto input)
    {
        // Validate duplicate code
        var normalizedCode = input.PlanCode?.Trim().ToUpperInvariant();
        if (!string.IsNullOrWhiteSpace(normalizedCode))
        {
            if (await PlanDefinitionRepository.IsCodeExistsAsync(normalizedCode))
            {
                throw new UserFriendlyException(
                    L["ProProductPlanDefinition:CodeExists"].Value.Replace("{Code}", normalizedCode));
            }
        }

        // Validate ProductId
        var product = await ProductRepository.GetAsync(input.ProductId);
        // Only able to create ProProductPlanDefinition if ProProduct.PartnerId = null
        if (product.PartnerId != null)
        {
            throw new UserFriendlyException(
                L["Product:ProProductPlanDefinition:ProductHasPartner", input.ProductId]);
        }

        var entity = new ProProductPlanDefinition(
            GuidGenerator.Create(),
            input.ProductId,
            input.PlanCode,
            input.PlanName,
            input.Status
        );

        await Manager.CreateAsync(entity);

        var dto = ObjectMapper.Map<ProProductPlanDefinition, ProProductPlanDefinitionDto>(entity);
        await PopulateCreatorNamesAsync(new[] { dto });
        return dto;
    }

    public override async Task<ProProductPlanDefinitionDto> UpdateAsync(Guid id, UpdateProProductPlanDefinitionDto input)
    {
        // Load entity with tracking to ensure EF Core can detect changes
        var entity = await Repository.GetAsync(id);

        // Validate ProductId
        var product = await ProductRepository.GetAsync(input.ProductId);
        // Only able to create ProProductPlanDefinition if ProProduct.PartnerId = null
        if (product.PartnerId != null)
        {
            throw new UserFriendlyException(
                L["Product:ProProductPlanDefinition:ProductHasPartner", input.ProductId]);
        }

        // ⚠️ QUAN TRỌNG: Chỉ update các fields được phép, không update PlanCode
        await Manager.UpdateAsync(
            entity,
            input.ProductId,
            input.PlanName,
            input.Status
        );

        // Ensure changes are saved
        await CurrentUnitOfWork.SaveChangesAsync();

        var dto = ObjectMapper.Map<ProProductPlanDefinition, ProProductPlanDefinitionDto>(entity);
        await PopulateCreatorNamesAsync(new[] { dto });
        return dto;
    }

    public override async Task DeleteAsync(Guid id)
    {
        // Repository.GetAsync will throw exception if not found, never returns null
        var entity = await Repository.GetAsync(id);

        if (await ProductRepository.AnyProductReferencesPlanDefinitionAsync(id))
        {
            var displayName = !string.IsNullOrWhiteSpace(entity.PlanName)
                ? entity.PlanName
                : entity.PlanCode;
            throw new UserFriendlyException(
                string.Format(
                    CultureInfo.CurrentCulture,
                    L["ProProductPlanDefinition:CannotDeleteInUse"].Value,
                    displayName));
        }

        // Delete to trigger audit logging with ChangeType = Deleted
        // FullAuditedAggregateRoot uses soft delete, so entity is not actually removed from database
        await Repository.DeleteAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
        
        // Update status to Deactive (entity still exists in database due to soft delete)
        entity!.UpdateStatus(ProProductPlanDefinitionStatus.Deactive);
        await Repository.UpdateAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    protected override async Task<IQueryable<ProProductPlanDefinition>> CreateFilteredQueryAsync(GetProProductPlanDefinitionsInput input)
    {
        var query = await ReadOnlyRepository.GetQueryableAsync();

        // Filter by PlanCode (case-insensitive, contains)
        if (!string.IsNullOrWhiteSpace(input.PlanCode))
        {
            query = query.Where(x => EF.Functions.ILike(x.PlanCode, $"%{input.PlanCode}%"));
        }

        // Filter by PlanName (case-insensitive, contains)
        if (!string.IsNullOrWhiteSpace(input.PlanName))
        {
            query = query.Where(x => EF.Functions.ILike(x.PlanName, $"%{input.PlanName}%"));
        }

        // Filter by Status
        if (input.Status.HasValue)
        {
            query = query.Where(x => x.Status == input.Status.Value);
        }

        // Filter by ProductId
        if (input.ProductId.HasValue)
        {
            query = query.Where(x => x.ProductId == input.ProductId.Value);
        }

        return query;
    }

    public virtual async Task<List<ProProductPlanDefinitionDto>> GetByProductIdAsync(Guid productId)
    {
        var query = await Repository.GetQueryableAsync();
        var entities = await query
            .Where(x => x.ProductId == productId)
            .ToListAsync();

        var dtos = ObjectMapper.Map<List<ProProductPlanDefinition>, List<ProProductPlanDefinitionDto>>(entities);
        await PopulateCreatorNamesAsync(dtos);
        return dtos;
    }

    private async Task PopulateCreatorNamesAsync(IReadOnlyList<ProProductPlanDefinitionDto> dtos)
    {
        var creatorIds = dtos
            .Where(d => d.CreatorId.HasValue)
            .Select(d => d.CreatorId!.Value)
            .Distinct()
            .ToList();
        if (creatorIds.Count == 0) return;

        var usersQuery = await UserRepository.GetQueryableAsync();
        var users = await usersQuery
            .Where(u => creatorIds.Contains(u.Id))
            .ToListAsync();
        var userDict = users.ToDictionary(u => u.Id, u => u);

        foreach (var dto in dtos)
        {
            if (dto.CreatorId.HasValue && userDict.TryGetValue(dto.CreatorId.Value, out var user))
            {
                dto.CreatorName = user.UserName;
            }
        }
    }
}
