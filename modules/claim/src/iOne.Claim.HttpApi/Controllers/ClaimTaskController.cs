using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Content;
using iOne.Claim.Claims;
using Volo.Abp;

namespace iOne.Claim.Controllers;

[RemoteService(Name = ClaimRemoteServiceConsts.RemoteServiceName)]
[Area(ClaimRemoteServiceConsts.ModuleName)]
[Route("api/claim-tasks")]
public class ClaimTaskController : AbpControllerBase
{
    protected IClaimTaskAppService AppService { get; }

    public ClaimTaskController(IClaimTaskAppService appService)
    {
        AppService = appService;
    }

    [HttpGet("{workTaskId:guid}/has-active-onsite-assessment")]
    public virtual Task<bool> HasActiveOnsiteAssessmentAsync(Guid workTaskId)
    {
        return AppService.HasActiveOnsiteAssessmentAsync(workTaskId);
    }

    [HttpGet("{workTaskId:guid}")]
    public virtual Task<ClaimTaskDto> GetAsync(Guid workTaskId)
    {
        return AppService.GetAsync(workTaskId);
    }

    [HttpGet("list")]
    public virtual Task<PagedResultDto<ClaimTaskDto>> GetListAsync([FromQuery] GetClaimTasksInput input)
    {
        return AppService.GetListAsync(input);
    }

    [HttpGet("object-types-by-policy-product")]
    public virtual Task<List<ClaimObjectTypeDto>> GetObjectTypesByPolicyAndProductAsync(
        [FromQuery] Guid policyId,
        [FromQuery] Guid productId,
        [FromQuery] DateTime? incidentDate)
    {
        return AppService.GetObjectTypesByPolicyAndProductAsync(policyId, productId, incidentDate);
    }

    [HttpPost("export")]
    public virtual Task<IRemoteStreamContent> ExportAsync([FromBody] GetClaimTasksInput input)
    {
        return AppService.ExportAsync(input);
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

    [HttpPut("{workTaskId:guid}/accept")]
    public virtual Task AcceptAsync(Guid workTaskId)
    {
        return AppService.AcceptAsync(workTaskId);
    }

    [HttpPut("{claimId:guid}/assign-onsite-assessment")]
    public virtual Task AssignOnsiteAssessmentAsync(Guid claimId, [FromBody] AssignOnsiteAssessmentInput input)
    {
        return AppService.AssignOnsiteAssessmentAsync(claimId, input);
    }

    [HttpPost("{workTaskId:guid}/open-folder")]
    public virtual Task OpenFolderAsync(Guid workTaskId, [FromBody] CreateClaimFolderDto input)
    {
        return AppService.OpenFolderAsync(workTaskId, input);
    }
}
