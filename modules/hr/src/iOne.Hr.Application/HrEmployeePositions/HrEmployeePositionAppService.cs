using System;
using System.Linq;
using System.Threading.Tasks;
using iOne.Hr;
using iOne.Hr.HrEmployeePositions;
using iOne.Hr.Localization;
using iOne.Hr.Permissions;
using iOne.HrEmployeePositions; // For Entity, Manager, Repository
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp;

namespace iOne.Hr.HrEmployeePositions;

[RemoteService(Name = HrRemoteServiceConsts.RemoteServiceName)]
[Authorize(HrEmployeePositionPermissions.Default)]
public class HrEmployeePositionAppService : CrudAppService<
    HrEmployeePosition,
    HrEmployeePositionDto,
    Guid,
    GetHrEmployeePositionsInput,
    CreateHrEmployeePositionDto,
    UpdateHrEmployeePositionDto>, IHrEmployeePositionAppService
{
    protected HrEmployeePositionManager Manager { get; }

    public HrEmployeePositionAppService(
        IHrEmployeePositionRepository repository,
        HrEmployeePositionManager manager)
        : base(repository)
    {
        Manager = manager;
        LocalizationResource = typeof(HrResource);
        GetPolicyName = HrEmployeePositionPermissions.View;
        GetListPolicyName = HrEmployeePositionPermissions.View;
        CreatePolicyName = HrEmployeePositionPermissions.Create;
        UpdatePolicyName = HrEmployeePositionPermissions.Edit;
        DeletePolicyName = HrEmployeePositionPermissions.Delete;
    }

    public override async Task<HrEmployeePositionDto> CreateAsync(CreateHrEmployeePositionDto input)
    {
        var entity = new HrEmployeePosition(
            GuidGenerator.Create(),
            input.Code,
            input.Name,
            input.Type,
            input.Status
        );

        await Manager.CreateAsync(entity);

        return ObjectMapper.Map<HrEmployeePosition, HrEmployeePositionDto>(entity);
    }

    public override async Task<HrEmployeePositionDto> UpdateAsync(Guid id, UpdateHrEmployeePositionDto input)
    {
        // Load entity with tracking to ensure EF Core can detect changes
        // Repository.GetAsync will throw exception if not found, never returns null
        var entity = await Repository.GetAsync(id);

        await Manager.UpdateAsync(entity, input.Name, input.Type, input.Status);

        // Ensure changes are saved (Repository.UpdateAsync should handle this, but explicitly save to ensure audit logging)
        await CurrentUnitOfWork.SaveChangesAsync();

        return ObjectMapper.Map<HrEmployeePosition, HrEmployeePositionDto>(entity);
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
        entity.UpdateStatus(HrEmployeePositionStatus.Deactive);
        await Repository.UpdateAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    protected override async Task<IQueryable<HrEmployeePosition>> CreateFilteredQueryAsync(GetHrEmployeePositionsInput input)
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

        // Filter by Type
        if (input.Type.HasValue)
        {
            query = query.Where(x => x.Type == input.Type.Value);
        }

        // Filter by Status
        if (input.Status.HasValue)
        {
            query = query.Where(x => x.Status == input.Status.Value);
        }

        return query;
    }
}

