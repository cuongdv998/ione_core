using System;
using System.Linq;
using System.Threading.Tasks;
using iOne.Product;
using iOne.Product.ProCoverageGroups;
using iOne.Product.Localization;
using iOne.Product.Permissions;
using iOne.ProCoverageGroups; // For Entity, Manager, Repository
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp;

namespace iOne.Product.ProCoverageGroups;

[RemoteService(Name = ProductRemoteServiceConsts.RemoteServiceName)]
[Authorize(ProCoverageGroupPermissions.Default)]
public class ProCoverageGroupAppService : CrudAppService<
    ProCoverageGroup,
    ProCoverageGroupDto,
    Guid,
    GetProCoverageGroupsInput,
    CreateProCoverageGroupDto,
    UpdateProCoverageGroupDto>, IProCoverageGroupAppService
{
    protected ProCoverageGroupManager Manager { get; }

    public ProCoverageGroupAppService(
        IProCoverageGroupRepository repository,
        ProCoverageGroupManager manager)
        : base(repository)
    {
        Manager = manager;
        LocalizationResource = typeof(ProductResource);
        GetPolicyName = ProCoverageGroupPermissions.View;
        GetListPolicyName = ProCoverageGroupPermissions.View;
        CreatePolicyName = ProCoverageGroupPermissions.Create;
        UpdatePolicyName = ProCoverageGroupPermissions.Edit;
        DeletePolicyName = ProCoverageGroupPermissions.Delete;
    }

    public override async Task<ProCoverageGroupDto> CreateAsync(CreateProCoverageGroupDto input)
    {
        var entity = new ProCoverageGroup(
            GuidGenerator.Create(),
            input.Code,
            input.Name,
            input.Status,
            input.Description
        );

        await Manager.CreateAsync(entity);

        return ObjectMapper.Map<ProCoverageGroup, ProCoverageGroupDto>(entity);
    }

    public override async Task<ProCoverageGroupDto> UpdateAsync(Guid id, UpdateProCoverageGroupDto input)
    {
        // Load entity with tracking to ensure EF Core can detect changes
        var entity = await Repository.GetAsync(id);

        await Manager.UpdateAsync(entity, input.Name, input.Status, input.Description);

        // Ensure changes are saved (Repository.UpdateAsync should handle this, but explicitly save to ensure audit logging)
        await CurrentUnitOfWork.SaveChangesAsync();

        return ObjectMapper.Map<ProCoverageGroup, ProCoverageGroupDto>(entity);
    }

    public override async Task DeleteAsync(Guid id)
    {
        var entity = await Repository.GetAsync(id);
        
        // Delete to trigger audit logging with ChangeType = Deleted
        // FullAuditedAggregateRoot uses soft delete, so entity is not actually removed from database
        await Repository.DeleteAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
        
        // Update status to Deactive (entity still exists in database due to soft delete)
        entity.UpdateStatus(ProCoverageGroupStatus.Deactive);
        await Repository.UpdateAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    protected override async Task<IQueryable<ProCoverageGroup>> CreateFilteredQueryAsync(GetProCoverageGroupsInput input)
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

        return query;
    }
}

