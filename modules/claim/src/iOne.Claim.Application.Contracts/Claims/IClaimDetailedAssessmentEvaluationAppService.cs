using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace iOne.Claim.Claims;

public interface IClaimDetailedAssessmentEvaluationAppService : IApplicationService
{
    Task<DetailedAssessmentEvaluationDetailDto> GetDetailAsync(Guid workTaskId);

    Task SaveAsync(Guid workTaskId, SaveDetailedAssessmentEvaluationInput input);
}
