using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using iOne.Claim.Claims;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Content;

namespace iOne.Claim.Controllers;

[RemoteService(Name = ClaimRemoteServiceConsts.RemoteServiceName)]
[Area(ClaimRemoteServiceConsts.ModuleName)]
[Route("api/claim-onsite-assessment-tasks")]
public class ClaimOnsiteAssessmentTaskController : AbpControllerBase
{
    protected IClaimOnsiteAssessmentTaskAppService AppService { get; }

    public ClaimOnsiteAssessmentTaskController(IClaimOnsiteAssessmentTaskAppService appService)
    {
        AppService = appService;
    }

    [HttpGet("{workTaskId:guid}/detail")]
    public virtual Task<OnsiteAssessmentDetailDto> GetDetailAsync(Guid workTaskId)
    {
        return AppService.GetDetailAsync(workTaskId);
    }

    [HttpGet("by-claim/{claimId:guid}/detail")]
    public virtual Task<OnsiteAssessmentDetailDto> GetDetailByClaimIdAsync(Guid claimId)
    {
        return AppService.GetDetailByClaimIdAsync(claimId);
    }

    [HttpGet("list")]
    public virtual Task<PagedResultDto<OnsiteAssessmentTaskDto>> GetListAsync([FromQuery] GetOnsiteAssessmentTasksInput input)
    {
        return AppService.GetListAsync(input);
    }

    [HttpPost("export")]
    public virtual Task<IRemoteStreamContent> ExportAsync([FromBody] GetOnsiteAssessmentTasksInput input)
    {
        return AppService.ExportAsync(input);
    }

    [HttpGet("create-request-list")]
    public virtual Task<List<OnsiteAssessmentCreateRequestDto>> GetCreateRequestListAsync()
    {
        return AppService.GetCreateRequestListAsync();
    }

    [HttpPost("create")]
    public virtual Task<OnsiteAssessmentCreateResultDto> CreateOnsiteAssessmentAsync([FromBody] CreateOnsiteAssessmentRequestInput input)
    {
        return AppService.CreateOnsiteAssessmentAsync(input);
    }

    [HttpPut("{workTaskId:guid}/cancel")]
    public virtual Task CancelAsync(Guid workTaskId)
    {
        return AppService.CancelAsync(workTaskId);
    }

    [HttpPut("{workTaskId:guid}/onsite/save")]
    public virtual Task SaveOnsiteAssessmentAsync(Guid workTaskId, [FromBody] SaveOnsiteAssessmentInput input)
    {
        return AppService.SaveOnsiteAssessmentAsync(workTaskId, input);
    }

    [HttpPut("{workTaskId:guid}/onsite/save-and-assign")]
    public virtual Task SaveAndAssignOnsiteAssessmentAsync(Guid workTaskId, [FromBody] SaveOnsiteAssessmentInput input)
    {
        return AppService.SaveAndAssignOnsiteAssessmentAsync(workTaskId, input);
    }

    [HttpDelete("{workTaskId:guid}/onsite/profile-files/{documentId:guid}")]
    public virtual Task RemoveOnsiteProfileFileAsync(Guid workTaskId, Guid documentId)
    {
        return AppService.RemoveOnsiteProfileFileAsync(workTaskId, documentId);
    }

    [HttpPut("{workTaskId:guid}/accept")]
    public virtual Task AcceptAsync(Guid workTaskId)
    {
        return AppService.AcceptAsync(workTaskId);
    }

    [HttpPut("{workTaskId:guid}/reject")]
    public virtual Task RejectAsync(Guid workTaskId, [FromBody] OnsiteRejectTaskInput input)
    {
        return AppService.RejectAsync(workTaskId, input);
    }

    [HttpPut("{workTaskId:guid}/transfer")]
    public virtual Task TransferAsync(Guid workTaskId, [FromBody] OnsiteTransferTaskInput input)
    {
        return AppService.TransferAsync(workTaskId, input);
    }

    [HttpPut("{workTaskId:guid}/reassign")]
    public virtual Task ReassignAsync(Guid workTaskId, [FromBody] ReassignOnsiteAssessmentInput input)
    {
        return AppService.ReassignAsync(workTaskId, input);
    }
}
