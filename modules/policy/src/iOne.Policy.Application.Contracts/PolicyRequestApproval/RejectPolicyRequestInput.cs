using System;
using System.ComponentModel.DataAnnotations;

namespace iOne.Policy.PolicyRequestApproval;

/// <summary>
/// Input for rejecting a policy creation request (reason and description are required).
/// </summary>
public class RejectPolicyRequestInput
{
    /// <summary>
    /// Reason ID from res_reason (group = APPROVAL_POLICY_REASON or TERMINATE_POLICY_REASON).
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
