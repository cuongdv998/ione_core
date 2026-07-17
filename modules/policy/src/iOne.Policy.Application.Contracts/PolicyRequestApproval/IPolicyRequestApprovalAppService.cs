using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Application.Dtos;
using iOne.Policy.Policies;

namespace iOne.Policy.PolicyRequestApproval;

/// <summary>
/// App service for policy request approval (create or terminate): list by work task assignee, approve/reject by work task id.
/// </summary>
public interface IPolicyRequestApprovalAppService : IApplicationService
{
    /// <summary>
    /// Gets the list of policies that the current user can approve (WorkTask.AssigneeId = current user's employee id).
    /// Each item includes WorkTaskId and WorkTaskStatus for navigation and approval status display.
    /// </summary>
    Task<PagedResultDto<PolicyRequestApprovalItemDto>> GetListAsync(GetPolicyRequestApprovalListInput input);

    /// <summary>
    /// Gets a single approval item by work task id (for detail page). Ensures the work task is assigned to the current user.
    /// </summary>
    Task<PolicyRequestApprovalItemDto> GetByWorkTaskIdAsync(Guid workTaskId);

    /// <summary>
    /// Approves the request identified by work task id. Work task must be assigned to current user and in WaitApprove status.
    /// </summary>
    Task ApproveAsync(Guid workTaskId);

    /// <summary>
    /// Rejects the request identified by work task id. Work task must be assigned to current user and in WaitApprove status.
    /// </summary>
    Task RejectAsync(Guid workTaskId, RejectPolicyRequestInput input);

    /// <summary>
    /// Approves multiple requests in one batch. All work tasks must be assigned to current user and in WaitApprove status.
    /// Persists in a single save; Elsa triggers are sent asynchronously with delay between each.
    /// </summary>
    Task ApproveBatchAsync(ApproveBatchRequest input);

    /// <summary>
    /// Rejects multiple requests in one batch with a shared reason. All work tasks must be assigned to current user and in WaitApprove status.
    /// Persists in a single save; Elsa triggers are sent asynchronously with delay between each.
    /// </summary>
    Task RejectBatchAsync(RejectBatchRequest input);
}
