using System;
using iOne.Policy.Policies;
using iOne.WorkTasks;

namespace iOne.Policy.PolicyRequestApproval;

/// <summary>
/// A policy in the request-approval list, with work task id and status so the client can navigate and call approve/reject by work task id.
/// </summary>
public class PolicyRequestApprovalItemDto
{
    /// <summary>
    /// Policy data (same shape as <see cref="PolicyDto"/>).
    /// </summary>
    public PolicyDto Policy { get; set; } = null!;

    /// <summary>
    /// Work task business code for this approval row, e.g. CREATE_POLICY_APPROVAL or TERMINATE_POLICY_APPROVAL.
    /// </summary>
    public string BusinessCode { get; set; } = null!;

    /// <summary>
    /// Policy version id (WorkTask.BusinessKey) – version đang chờ phê duyệt; dùng khi xem chi tiết theo version.
    /// </summary>
    public Guid PolicyVersionId { get; set; }

    /// <summary>
    /// Work task id; use for navigation and for ApproveAsync/RejectAsync.
    /// </summary>
    public Guid WorkTaskId { get; set; }

    /// <summary>
    /// Work task status (displayed as approval status in the list).
    /// </summary>
    public WorkTaskStatus WorkTaskStatus { get; set; }

    /// <summary>
    /// Status of the latest policy version (for display and row selectability).
    /// </summary>
    public string? PolicyVersionStatus { get; set; }
}
