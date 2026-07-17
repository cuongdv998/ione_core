using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Content;

namespace iOne.Claim.Claims;

public interface IClaimOnsiteAssessmentTaskAppService : IApplicationService
{
    Task<OnsiteAssessmentDetailDto> GetDetailAsync(Guid workTaskId);

    Task<OnsiteAssessmentDetailDto> GetDetailByClaimIdAsync(Guid claimId);

    Task<PagedResultDto<OnsiteAssessmentTaskDto>> GetListAsync(GetOnsiteAssessmentTasksInput input);

    Task<IRemoteStreamContent> ExportAsync(GetOnsiteAssessmentTasksInput input);

    Task<List<OnsiteAssessmentCreateRequestDto>> GetCreateRequestListAsync();

    Task<OnsiteAssessmentCreateResultDto> CreateOnsiteAssessmentAsync(CreateOnsiteAssessmentRequestInput input);

    Task CancelAsync(Guid workTaskId);

    Task SaveOnsiteAssessmentAsync(Guid workTaskId, SaveOnsiteAssessmentInput input);

    Task SaveAndAssignOnsiteAssessmentAsync(Guid workTaskId, SaveOnsiteAssessmentInput input);

    Task RemoveOnsiteProfileFileAsync(Guid workTaskId, Guid documentId);

    Task AcceptAsync(Guid workTaskId);

    Task RejectAsync(Guid workTaskId, OnsiteRejectTaskInput input);

    Task TransferAsync(Guid workTaskId, OnsiteTransferTaskInput input);

    Task ReassignAsync(Guid workTaskId, ReassignOnsiteAssessmentInput input);
}
