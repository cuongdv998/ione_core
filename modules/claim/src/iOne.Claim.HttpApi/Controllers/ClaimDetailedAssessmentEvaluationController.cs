using System;
using System.Threading.Tasks;
using iOne.Claim.Claims;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;

namespace iOne.Claim.Controllers;

[Route("api/claim-detailed-assessment-evaluation")]
public class ClaimDetailedAssessmentEvaluationController : AbpControllerBase
{
    protected IClaimDetailedAssessmentEvaluationAppService AppService { get; }

    public ClaimDetailedAssessmentEvaluationController(IClaimDetailedAssessmentEvaluationAppService appService)
    {
        AppService = appService;
    }

    [HttpGet("{workTaskId:guid}/detail")]
    public virtual Task<DetailedAssessmentEvaluationDetailDto> GetDetailAsync(Guid workTaskId)
    {
        return AppService.GetDetailAsync(workTaskId);
    }

    [HttpPut("{workTaskId:guid}/save")]
    public virtual Task SaveAsync(Guid workTaskId, [FromBody] SaveDetailedAssessmentEvaluationInput input)
    {
        return AppService.SaveAsync(workTaskId, input);
    }
}
