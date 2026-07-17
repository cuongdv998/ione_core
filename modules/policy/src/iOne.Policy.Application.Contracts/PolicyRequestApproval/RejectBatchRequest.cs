using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace iOne.Policy.PolicyRequestApproval;

/// <summary>
/// Input for rejecting multiple policy request approvals in one batch with a shared reason.
/// </summary>
public class RejectBatchRequest
{
    /// <summary>
    /// Work task IDs to reject. All must be assigned to the current user and in WaitApprove status.
    /// </summary>
    [Required]
    [MinLength(1)]
    public List<Guid> WorkTaskIds { get; set; } = new();

    /// <summary>
    /// Reason ID from res_reason (group = APPROVAL_POLICY_REASON or TERMINATE_POLICY_REASON or ENDORSEMENT).
    /// </summary>
    [Required]
    public Guid? ReasonId { get; set; }

    /// <summary>
    /// Free-text description for the rejection (required).
    /// </summary>
    [Required]
    [MaxLength(250)]
    public string? ReasonDescription { get; set; }
}
