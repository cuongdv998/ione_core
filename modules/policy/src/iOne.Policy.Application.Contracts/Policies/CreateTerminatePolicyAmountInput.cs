using System;

namespace iOne.Policy.Policies;

/// <summary>
/// Input for creating a PolicyAmount record for policy termination (refund amount).
/// Used by the termination workflow when it calls POST /api/policy/create-terminate-policy-amount.
/// </summary>
public class CreateTerminatePolicyAmountInput
{
    /// <summary>
    /// The policy version id (version for which the termination amount is created).
    /// </summary>
    public Guid PolicyVersionId { get; set; }

    /// <summary>
    /// Total termination/refund amount. Stored as absolute value in PolicyAmount.
    /// </summary>
    public decimal TotalTerminationAmount { get; set; }

    /// <summary>
    /// Optional termination date used as PolicyAmount.IssueDate. If null, backend uses current date.
    /// </summary>
    public DateTime? TerminationDate { get; set; }
}
