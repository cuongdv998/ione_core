using System;
using System.Linq;
using System.Threading.Tasks;
using iOne.Hr;
using iOne.Hr.HrEmployeeLevels;
using iOne.Hr.Localization;
using iOne.Hr.Permissions;
using iOne.HrEmployeeLevels; // For Entity, Manager, Repository
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp;

namespace iOne.Hr.HrEmployeeLevels;

[RemoteService(Name = HrRemoteServiceConsts.RemoteServiceName)]
[Authorize(HrEmployeeLevelPermissions.Default)]
public class HrEmployeeLevelAppService : CrudAppService<
    HrEmployeeLevel,
    HrEmployeeLevelDto,
    Guid,
    GetHrEmployeeLevelsInput,
    CreateHrEmployeeLevelDto,
    UpdateHrEmployeeLevelDto>, IHrEmployeeLevelAppService
{
    protected HrEmployeeLevelManager Manager { get; }

    public HrEmployeeLevelAppService(
        IHrEmployeeLevelRepository repository,
        HrEmployeeLevelManager manager)
        : base(repository)
    {
        Manager = manager;
        LocalizationResource = typeof(HrResource);
        GetPolicyName = HrEmployeeLevelPermissions.View;
        GetListPolicyName = HrEmployeeLevelPermissions.View;
        CreatePolicyName = HrEmployeeLevelPermissions.Create;
        UpdatePolicyName = HrEmployeeLevelPermissions.Edit;
        DeletePolicyName = HrEmployeeLevelPermissions.Delete;
    }

    public override async Task<HrEmployeeLevelDto> CreateAsync(CreateHrEmployeeLevelDto input)
    {
        var entity = new HrEmployeeLevel(
            GuidGenerator.Create(),
            input.Code,
            input.Name,
            input.Status,
            input.Description
        );

        await Manager.CreateAsync(entity);

        return ObjectMapper.Map<HrEmployeeLevel, HrEmployeeLevelDto>(entity);
    }

    public override async Task<HrEmployeeLevelDto> UpdateAsync(Guid id, UpdateHrEmployeeLevelDto input)
    {
        // Load entity with tracking to ensure EF Core can detect changes
        var entity = await Repository.GetAsync(id);

        await Manager.UpdateAsync(entity, input.Name, input.Status, input.Description);

        // Ensure changes are saved (Repository.UpdateAsync should handle this, but explicitly save to ensure audit logging)
        await CurrentUnitOfWork.SaveChangesAsync();

        return ObjectMapper.Map<HrEmployeeLevel, HrEmployeeLevelDto>(entity);
    }

    public override async Task DeleteAsync(Guid id)
    {
        var entity = await Repository.GetAsync(id);
        
        // Delete to trigger audit logging with ChangeType = Deleted
        // FullAuditedAggregateRoot uses soft delete, so entity is not actually removed from database
        await Repository.DeleteAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
        
        // Update status to Deactive (entity still exists in database due to soft delete)
        entity.UpdateStatus(HrEmployeeLevelStatus.Deactive);
        await Repository.UpdateAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    protected override async Task<IQueryable<HrEmployeeLevel>> CreateFilteredQueryAsync(GetHrEmployeeLevelsInput input)
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

