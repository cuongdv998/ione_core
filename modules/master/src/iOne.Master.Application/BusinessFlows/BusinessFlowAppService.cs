using System;
using System.Linq;
using System.Threading.Tasks;
using iOne.BusinessFlows;
using iOne.Master;
using iOne.Master.Localization;
using iOne.Master.Permissions;
using iOne.Master.BusinessFlows;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace iOne.Master.BusinessFlows;

[RemoteService(Name = MasterRemoteServiceConsts.RemoteServiceName)]
[Authorize(BusinessFlowPermissions.Default)]
public class BusinessFlowAppService : CrudAppService<
    BusinessFlow,
    BusinessFlowDto,
    Guid,
    GetBusinessFlowsInput,
    CreateBusinessFlowDto,
    UpdateBusinessFlowDto>, IBusinessFlowAppService
{
    protected BusinessFlowManager Manager { get; }
    protected IBusinessFlowRepository FlowRepository { get; }

    public BusinessFlowAppService(
        IBusinessFlowRepository repository,
        BusinessFlowManager manager)
        : base(repository)
    {
        Manager = manager;
        FlowRepository = repository;
        LocalizationResource = typeof(MasterResource);
        GetPolicyName = BusinessFlowPermissions.View;
        GetListPolicyName = BusinessFlowPermissions.View;
        CreatePolicyName = BusinessFlowPermissions.Create;
        UpdatePolicyName = BusinessFlowPermissions.Edit;
        DeletePolicyName = BusinessFlowPermissions.Delete;
    }

    public override async Task<BusinessFlowDto> CreateAsync(CreateBusinessFlowDto input)
    {
        var entity = new BusinessFlow(
            GuidGenerator.Create(),
            input.OrganizationId,
            input.InsurerId,
            input.BusinessCode,
            input.WorkflowName,
            input.WorkflowVersion,
            input.EffectDate,
            input.ExpireDate,
            BusinessFlowStatus.Active
        );

        await Manager.CreateAsync(entity);

        return ObjectMapper.Map<BusinessFlow, BusinessFlowDto>(entity);
    }

    public override async Task<BusinessFlowDto> UpdateAsync(Guid id, UpdateBusinessFlowDto input)
    {
        var entity = await Repository.GetAsync(id);

        await Manager.UpdateWorkflowInfoAsync(
            entity,
            input.WorkflowName,
            input.WorkflowVersion,
            input.ExpireDate);

        await CurrentUnitOfWork.SaveChangesAsync();

        return ObjectMapper.Map<BusinessFlow, BusinessFlowDto>(entity);
    }

    public override async Task DeleteAsync(Guid id)
    {
        var entity = await Repository.GetAsync(id);

        await Repository.DeleteAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();

        entity.UpdateStatus(BusinessFlowStatus.Deactive);
        await Repository.UpdateAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    protected override async Task<IQueryable<BusinessFlow>> CreateFilteredQueryAsync(GetBusinessFlowsInput input)
    {
        var query = await ReadOnlyRepository.GetQueryableAsync();

        if (input.OrganizationId.HasValue)
        {
            query = query.Where(x => x.OrganizationId == input.OrganizationId.Value);
        }

        if (input.InsurerId.HasValue)
        {
            query = query.Where(x => x.InsurerId == input.InsurerId.Value);
        }

        if (!string.IsNullOrWhiteSpace(input.BusinessCode))
        {
            query = query.Where(x => EF.Functions.ILike(x.BusinessCode, $"%{input.BusinessCode}%"));
        }

        if (!string.IsNullOrWhiteSpace(input.WorkflowName))
        {
            query = query.Where(x => EF.Functions.ILike(x.WorkflowName, $"%{input.WorkflowName}%"));
        }

        if (!string.IsNullOrWhiteSpace(input.WorkflowVersion))
        {
            query = query.Where(x => EF.Functions.ILike(x.WorkflowVersion, $"%{input.WorkflowVersion}%"));
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

        if (input.ExpireDateFrom.HasValue)
        {
            query = query.Where(x => x.ExpireDate != null && x.ExpireDate >= input.ExpireDateFrom.Value);
        }

        if (input.ExpireDateTo.HasValue)
        {
            query = query.Where(x => x.ExpireDate != null && x.ExpireDate <= input.ExpireDateTo.Value);
        }

        return query;
    }
}
