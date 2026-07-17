using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;
using iOne.Policy.Permissions;
using iOne.Policy.Policies;
using iOne.Policy.PolicyRequestApproval;

namespace iOne.Policy.Controllers;

[Authorize]
[RemoteService(Name = PolicyRemoteServiceConsts.RemoteServiceName)]
[Area(PolicyRemoteServiceConsts.ModuleName)]
[Route("api/policy/request-approval")]
public class PolicyRequestApprovalController : AbpControllerBase
{
    protected IPolicyRequestApprovalAppService AppService { get; }

    public PolicyRequestApprovalController(IPolicyRequestApprovalAppService appService)
    {
        AppService = appService;
    }

    [HttpGet]
    public virtual Task<PagedResultDto<PolicyRequestApprovalItemDto>> GetListAsync([FromQuery] GetPolicyRequestApprovalListInput input)
    {
        return AppService.GetListAsync(input);
    }

    [HttpGet("{workTaskId}")]
    public virtual Task<PolicyRequestApprovalItemDto> GetByWorkTaskIdAsync(Guid workTaskId)
    {
        return AppService.GetByWorkTaskIdAsync(workTaskId);
    }

    [HttpPut("{workTaskId}/approve")]
    public virtual Task ApproveAsync(Guid workTaskId)
    {
        return AppService.ApproveAsync(workTaskId);
    }

    [HttpPut("{workTaskId}/reject")]
    public virtual Task RejectAsync(Guid workTaskId, [FromBody] RejectPolicyRequestInput input)
    {
        return AppService.RejectAsync(workTaskId, input);
    }

    [HttpPut("approve-batch")]
    public virtual Task ApproveBatchAsync([FromBody] ApproveBatchRequest input)
    {
        return AppService.ApproveBatchAsync(input);
    }

    [HttpPut("reject-batch")]
    public virtual Task RejectBatchAsync([FromBody] RejectBatchRequest input)
    {
        return AppService.RejectBatchAsync(input);
    }
}
