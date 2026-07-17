using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace iOne.Policy.Policies;

/// <summary>
/// Input DTO for confirming policy termination.
/// </summary>
public class TerminatePolicyInput
{
    /// <summary>
    /// Termination reason ID (from res_reason table).
    /// </summary>
    public Guid TerminationReasonId { get; set; }

    /// <summary>
    /// Optional description for the termination reason.
    /// </summary>
    public string? TerminationReasonDescription { get; set; }

    /// <summary>
    /// The date when the policy is terminated.
    /// </summary>
    public DateTime TerminationDate { get; set; }

    /// <summary>
    /// Total refund amount (negative value indicates money to be refunded to customer).
    /// </summary>
    public decimal TotalRefundAmount { get; set; }

    /// <summary>
    /// Optional internal note saved on the current policy version.
    /// </summary>
    [StringLength(500, ErrorMessage = "PolicyVersion:InternalNoteMaxLength")]
    public string? InternalNote { get; set; }

    /// <summary>
    /// Optional customer note saved on the current policy version.
    /// </summary>
    [StringLength(500, ErrorMessage = "PolicyVersion:CustomerNoteMaxLength")]
    public string? CustomerNote { get; set; }

    /// <summary>
    /// Refund amounts distributed to each coverage.
    /// </summary>
    public List<CoverageRefundDto>? CoverageRefunds { get; set; }

    /// <summary>
    /// Policy-level attachments to keep after termination is submitted.
    /// </summary>
    public List<UpdatePolicyDocumentInputDto>? Documents { get; set; }
}
