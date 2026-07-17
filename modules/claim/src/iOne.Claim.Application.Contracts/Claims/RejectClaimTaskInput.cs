using System;
using System.ComponentModel.DataAnnotations;

namespace iOne.Claim.Claims;

/// <summary>
/// Input for rejecting a claim task (reason and description are required).
/// </summary>
public class RejectClaimTaskInput
{
    /// <summary>
    /// Reason ID from res_reason (group = CLAIM_REJECTION_REASON).
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
