using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Content;

namespace iOne.Claim.Claims;

/// <summary>
/// App service cho danh sách yêu cầu bồi thường được giao cho người đăng nhập (theo work_task).
/// </summary>
public interface IClaimTaskAppService : IApplicationService
{
    Task<ClaimTaskDto> GetAsync(Guid workTaskId);

    Task<PagedResultDto<ClaimTaskDto>> GetListAsync(GetClaimTasksInput input);

    Task<IRemoteStreamContent> ExportAsync(GetClaimTasksInput input);

    Task RejectAsync(Guid workTaskId, RejectClaimTaskInput input);

    Task TransferAsync(Guid workTaskId, TransferClaimTaskInput input);

    Task AcceptAsync(Guid workTaskId);

    Task AssignOnsiteAssessmentAsync(Guid claimId, AssignOnsiteAssessmentInput input);

    Task<bool> HasActiveOnsiteAssessmentAsync(Guid workTaskId);

    Task OpenFolderAsync(Guid workTaskId, CreateClaimFolderDto input);

    Task<List<ClaimObjectTypeDto>> GetObjectTypesByPolicyAndProductAsync(Guid policyId, Guid productId, DateTime? incidentDate);
}
