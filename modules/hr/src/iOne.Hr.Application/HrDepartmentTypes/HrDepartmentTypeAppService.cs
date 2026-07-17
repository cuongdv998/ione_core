using System;
using System.Linq;
using System.Threading.Tasks;
using iOne.Hr;
using iOne.Hr.HrDepartmentTypes;
using iOne.Hr.Localization;
using iOne.Hr.Permissions;
using iOne.HrDepartmentTypes; // For Entity, Manager, Repository
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp;

namespace iOne.Hr.HrDepartmentTypes;

[RemoteService(Name = HrRemoteServiceConsts.RemoteServiceName)]
[Authorize(HrDepartmentTypePermissions.Default)]
public class HrDepartmentTypeAppService : CrudAppService<
    HrDepartmentType,
    HrDepartmentTypeDto,
    Guid,
    GetHrDepartmentTypesInput,
    CreateHrDepartmentTypeDto,
    UpdateHrDepartmentTypeDto>, IHrDepartmentTypeAppService
{
    protected HrDepartmentTypeManager Manager { get; }

    public HrDepartmentTypeAppService(
        IHrDepartmentTypeRepository repository,
        HrDepartmentTypeManager manager)
        : base(repository)
    {
        Manager = manager;
        LocalizationResource = typeof(HrResource);
        GetPolicyName = HrDepartmentTypePermissions.View;
        GetListPolicyName = HrDepartmentTypePermissions.View;
        CreatePolicyName = HrDepartmentTypePermissions.Create;
        UpdatePolicyName = HrDepartmentTypePermissions.Edit;
        DeletePolicyName = HrDepartmentTypePermissions.Delete;
    }

    public override async Task<HrDepartmentTypeDto> CreateAsync(CreateHrDepartmentTypeDto input)
    {
        var entity = new HrDepartmentType(
            GuidGenerator.Create(),
            input.Code,
            input.Name,
            input.Status
        );

        await Manager.CreateAsync(entity);

        return ObjectMapper.Map<HrDepartmentType, HrDepartmentTypeDto>(entity);
    }

    public override async Task<HrDepartmentTypeDto> UpdateAsync(Guid id, UpdateHrDepartmentTypeDto input)
    {
        // Load entity with tracking to ensure EF Core can detect changes
        // Repository.GetAsync will throw exception if not found, never returns null
        var entity = await Repository.GetAsync(id);

        await Manager.UpdateAsync(entity, input.Name, input.Status);

        // Ensure changes are saved (Repository.UpdateAsync should handle this, but explicitly save to ensure audit logging)
        await CurrentUnitOfWork.SaveChangesAsync();

        return ObjectMapper.Map<HrDepartmentType, HrDepartmentTypeDto>(entity);
    }

    public override async Task DeleteAsync(Guid id)
    {
        // Repository.GetAsync will throw exception if not found, never returns null
        var entity = await Repository.GetAsync(id);
        
        // Delete to trigger audit logging with ChangeType = Deleted
        // FullAuditedAggregateRoot uses soft delete, so entity is not actually removed from database
        await Repository.DeleteAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
        
        // Update status to Deactive (entity still exists in database due to soft delete)
        entity.UpdateStatus(HrDepartmentTypeStatus.Deactive);
        await Repository.UpdateAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    protected override async Task<IQueryable<HrDepartmentType>> CreateFilteredQueryAsync(GetHrDepartmentTypesInput input)
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

