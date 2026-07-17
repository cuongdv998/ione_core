using System;
using System.ComponentModel.DataAnnotations;

namespace iOne.Claim.Claims;

/// <summary>
/// Input for transferring a claim task to another department/assignee.
/// </summary>
public class TransferClaimTaskInput
{
    /// <summary>
    /// Reason ID from res_reason (group = CLAIM_TRANSFER_REASON).
    /// </summary>
    [Required]
    public Guid? ReasonId { get; set; }

    /// <summary>
    /// Free-text description for the transfer (required).
    /// </summary>
    [Required]
    [MaxLength(250)]
    public string? ReasonDescription { get; set; }

    /// <summary>
    /// Department ID of the receiving unit.
    /// </summary>
    [Required]
    public Guid DepartmentId { get; set; }

    /// <summary>
    /// Employee ID of the new assignee.
    /// </summary>
    [Required]
    public Guid AssigneeId { get; set; }
}
