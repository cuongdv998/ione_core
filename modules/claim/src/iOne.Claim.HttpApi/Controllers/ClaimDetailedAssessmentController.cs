using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using iOne.Claim.Claims;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;

namespace iOne.Claim.Controllers;

[RemoteService(Name = ClaimRemoteServiceConsts.RemoteServiceName)]
[Area(ClaimRemoteServiceConsts.ModuleName)]
[Route("api/claim-detailed-assessment")]
public class ClaimDetailedAssessmentController : AbpControllerBase
{
    protected IClaimDetailedAssessmentAppService AppService { get; }

    public ClaimDetailedAssessmentController(IClaimDetailedAssessmentAppService appService)
    {
        AppService = appService;
    }

    [HttpGet("{workTaskId:guid}/detail")]
    public virtual Task<DetailedAssessmentDetailDto> GetDetailAsync(Guid workTaskId)
    {
        return AppService.GetDetailAsync(workTaskId);
    }

    [HttpGet("{workTaskId:guid}/coverage-options")]
    public virtual Task<List<DetailedAssessmentOptionDto>> GetCoverageOptionsAsync(Guid workTaskId)
    {
        return AppService.GetCoverageOptionsAsync(workTaskId);
    }

    [HttpPut("{workTaskId:guid}/save")]
    public virtual Task SaveAsync(Guid workTaskId, [FromBody] SaveDetailedAssessmentInput input)
    {
        return AppService.SaveAsync(workTaskId, input);
    }

    [HttpPut("{workTaskId:guid}/complete")]
    public virtual Task CompleteAsync(Guid workTaskId, [FromBody] SaveDetailedAssessmentInput input)
    {
        return AppService.CompleteAsync(workTaskId, input);
    }

    [HttpPut("{workTaskId:guid}/accept")]
    public virtual Task AcceptAsync(Guid workTaskId)
    {
        return AppService.AcceptAsync(workTaskId);
    }

    [HttpPut("{workTaskId:guid}/cancel")]
    public virtual Task CancelAsync(Guid workTaskId)
    {
        return AppService.CancelAsync(workTaskId);
    }

    [HttpPut("{workTaskId:guid}/reject")]
    public virtual Task RejectAsync(Guid workTaskId, [FromBody] RejectClaimTaskInput input)
    {
        return AppService.RejectAsync(workTaskId, input);
    }

    [HttpPut("{workTaskId:guid}/transfer")]
    public virtual Task TransferAsync(Guid workTaskId, [FromBody] TransferClaimTaskInput input)
    {
        return AppService.TransferAsync(workTaskId, input);
    }

    [HttpPut("{workTaskId:guid}/reassign")]
    public virtual Task ReassignAsync(Guid workTaskId, [FromBody] ReassignDetailedAssessmentInput input)
    {
        return AppService.ReassignAsync(workTaskId, input);
    }
}
