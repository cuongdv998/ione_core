using System;
using System.Linq;
using System.Threading.Tasks;
using iOne.Master;
using iOne.Master.Localization;
using iOne.Master.Permissions;
using iOne.Master.ResBusinessAssignees;
using iOne.ResBusinessAssignees;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace iOne.Master.ResBusinessAssignees;

[RemoteService(Name = MasterRemoteServiceConsts.RemoteServiceName)]
[Authorize(ResBusinessAssigneePermissions.Default)]
public class ResBusinessAssigneeAppService : CrudAppService<
    ResBusinessAssignee,
    ResBusinessAssigneeDto,
    Guid,
    GetResBusinessAssigneesInput,
    CreateResBusinessAssigneeDto,
    UpdateResBusinessAssigneeDto>,
    IResBusinessAssigneeAppService
{
    protected ResBusinessAssigneeManager Manager { get; }

    public ResBusinessAssigneeAppService(
        IResBusinessAssigneeRepository repository,
        ResBusinessAssigneeManager manager)
        : base(repository)
    {
        Manager = manager;
        LocalizationResource = typeof(MasterResource);
        GetPolicyName = ResBusinessAssigneePermissions.View;
        GetListPolicyName = ResBusinessAssigneePermissions.View;
        CreatePolicyName = ResBusinessAssigneePermissions.Create;
        UpdatePolicyName = ResBusinessAssigneePermissions.Edit;
        DeletePolicyName = ResBusinessAssigneePermissions.Delete;
    }

    public override async Task<ResBusinessAssigneeDto> CreateAsync(CreateResBusinessAssigneeDto input)
    {
        var entity = new ResBusinessAssignee(
            GuidGenerator.Create(),
            input.BusinessCode,
            input.AuthorityCode,
            input.AssigneeType,
            input.EffectDate,
            input.Status,
            input.OrganizationId,
            input.ExpireDate,
            input.AssigneeRole,
            input.AssigneeId,
            input.DepartmentId,
            input.DepartmentLevel
        );

        await Manager.CreateAsync(entity);

        return ObjectMapper.Map<ResBusinessAssignee, ResBusinessAssigneeDto>(entity);
    }

    public override async Task<ResBusinessAssigneeDto> UpdateAsync(Guid id, UpdateResBusinessAssigneeDto input)
    {
        var entity = await Repository.GetAsync(id);

        await Manager.UpdateAsync(entity, input.AssigneeId, input.ExpireDate);

        await CurrentUnitOfWork.SaveChangesAsync();

        return ObjectMapper.Map<ResBusinessAssignee, ResBusinessAssigneeDto>(entity);
    }

    public override async Task DeleteAsync(Guid id)
    {
        var entity = await Repository.GetAsync(id);

        await Repository.DeleteAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();

        entity.UpdateStatus(ResBusinessAssigneeStatus.Deactive);
        await Repository.UpdateAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    protected override async Task<IQueryable<ResBusinessAssignee>> CreateFilteredQueryAsync(GetResBusinessAssigneesInput input)
    {
        var query = await ReadOnlyRepository.GetQueryableAsync();

        if (input.OrganizationId.HasValue)
        {
            query = query.Where(x => x.OrganizationId == input.OrganizationId.Value);
        }

        if (!string.IsNullOrWhiteSpace(input.BusinessCode))
        {
            query = query.Where(x => EF.Functions.ILike(x.BusinessCode, $"%{input.BusinessCode}%"));
        }

        if (!string.IsNullOrWhiteSpace(input.AuthorityCode))
        {
            query = query.Where(x => EF.Functions.ILike(x.AuthorityCode, $"%{input.AuthorityCode}%"));
        }

        if (input.AssigneeType.HasValue)
        {
            query = query.Where(x => x.AssigneeType == input.AssigneeType.Value);
        }

        if (input.DepartmentId.HasValue)
        {
            query = query.Where(x => x.DepartmentId == input.DepartmentId.Value);
        }

        if (input.AssigneeId.HasValue)
        {
            query = query.Where(x => x.AssigneeId == input.AssigneeId.Value);
        }

        if (input.Status.HasValue)
        {
            query = query.Where(x => x.Status == input.Status.Value);
        }

        if (input.EffectDateFrom.HasValue)
        {
            query = query.Where(x => x.EffectDate >= input.EffectDateFrom.Value);
        }

        if (input.EffectDateTo.HasValue)
        {
            query = query.Where(x => x.EffectDate <= input.EffectDateTo.Value);
        }

        return query;
    }
}
