using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace iOne.Claim.Claims;

public interface IClaimDetailedAssessmentAppService : IApplicationService
{
    Task<DetailedAssessmentDetailDto> GetDetailAsync(Guid workTaskId);

    Task<List<DetailedAssessmentOptionDto>> GetCoverageOptionsAsync(Guid workTaskId);

    Task SaveAsync(Guid workTaskId, SaveDetailedAssessmentInput input);

    Task CompleteAsync(Guid workTaskId, SaveDetailedAssessmentInput input);

    Task AcceptAsync(Guid workTaskId);

    Task CancelAsync(Guid workTaskId);

    Task RejectAsync(Guid workTaskId, RejectClaimTaskInput input);

    Task TransferAsync(Guid workTaskId, TransferClaimTaskInput input);

    Task ReassignAsync(Guid workTaskId, ReassignDetailedAssessmentInput input);
}
