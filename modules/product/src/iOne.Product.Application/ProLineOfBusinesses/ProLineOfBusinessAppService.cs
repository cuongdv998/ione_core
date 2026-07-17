using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using iOne.Product;
using iOne.Product.ProLineOfBusinesses;
using iOne.Product.Localization;
using iOne.Product.Permissions;
using iOne.ProLineOfBusinesses; // For Entity, Manager, Repository
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp;

namespace iOne.Product.ProLineOfBusinesses;

[RemoteService(Name = ProductRemoteServiceConsts.RemoteServiceName)]
[Authorize]
public class ProLineOfBusinessAppService : CrudAppService<
    ProLineOfBusiness,
    ProLineOfBusinessDto,
    Guid,
    GetProLineOfBusinessesInput,
    CreateProLineOfBusinessDto,
    UpdateProLineOfBusinessDto>, IProLineOfBusinessAppService
{
    protected ProLineOfBusinessManager Manager { get; }

    public ProLineOfBusinessAppService(
        IProLineOfBusinessRepository repository,
        ProLineOfBusinessManager manager)
        : base(repository)
    {
        Manager = manager;
        LocalizationResource = typeof(ProductResource);
        GetPolicyName = ProLineOfBusinessPermissions.View;
        GetListPolicyName = ProLineOfBusinessPermissions.View;
        CreatePolicyName = ProLineOfBusinessPermissions.Create;
        UpdatePolicyName = ProLineOfBusinessPermissions.Edit;
        DeletePolicyName = ProLineOfBusinessPermissions.Delete;
    }

    public override async Task<ProLineOfBusinessDto> CreateAsync(CreateProLineOfBusinessDto input)
    {
        var entity = new ProLineOfBusiness(
            GuidGenerator.Create(),
            input.Code,
            input.Name,
            input.Status,
            input.ParentId,
            input.Description
        );

        await Manager.CreateAsync(entity);

        return ObjectMapper.Map<ProLineOfBusiness, ProLineOfBusinessDto>(entity);
    }

    public override async Task<ProLineOfBusinessDto> UpdateAsync(Guid id, UpdateProLineOfBusinessDto input)
    {
        // Load entity with tracking to ensure EF Core can detect changes
        var entity = await Repository.GetAsync(id);

        await Manager.UpdateAsync(entity, input.Name, input.Status, input.ParentId, input.Description);

        // Ensure changes are saved (Repository.UpdateAsync should handle this, but explicitly save to ensure audit logging)
        await CurrentUnitOfWork.SaveChangesAsync();

        return ObjectMapper.Map<ProLineOfBusiness, ProLineOfBusinessDto>(entity);
    }

    public override async Task DeleteAsync(Guid id)
    {
        var entity = await Repository.GetAsync(id);

        // Delete to trigger audit logging with ChangeType = Deleted
        // FullAuditedAggregateRoot uses soft delete, so entity is not actually removed from database
        await Repository.DeleteAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();

        // Update status to Deactive (entity still exists in database due to soft delete)
        entity.UpdateStatus(ProLineOfBusinessStatus.Deactive);
        await Repository.UpdateAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    protected override async Task<IQueryable<ProLineOfBusiness>> CreateFilteredQueryAsync(GetProLineOfBusinessesInput input)
    {
        var query = await ReadOnlyRepository.GetQueryableAsync();

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

        // Filter by ParentId
        if (input.ParentId.HasValue)
        {
            query = query.Where(x => x.ParentId == input.ParentId.Value);
        }
        else if (input.ParentId == Guid.Empty)
        {
            // Filter for root items (no parent)
            query = query.Where(x => x.ParentId == null);
        }

        return query;
    }

    [Authorize]
    public virtual async Task<List<ProLineOfBusinessSelectDto>> GetSelectListAsync()
    {
        var query = await ReadOnlyRepository.GetQueryableAsync();
        
        // Sort by Name ascending
        query = query.OrderBy(x => x.Name);
        
        // Execute query and get entities
        var entities = await AsyncExecuter.ToListAsync(query);
        
        // Map entities to Select DTOs (only Id, Code, Name)
        var dtos = entities.Select(x => new ProLineOfBusinessSelectDto
        {
            Id = x.Id,
            Code = x.Code,
            Name = x.Name
        }).ToList();
        
        return dtos;
    }
}




