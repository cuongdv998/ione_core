using System;
using System.Collections.Generic;

namespace iOne.Policy.PolicyContracts;

/// <summary>
/// Input for terminating a contract and all its active policies.
/// </summary>
public class TerminateContractInput
{
    public Guid ContractId { get; set; }

    /// <summary>
    /// Termination reason ID (from res_reason table).
    /// </summary>
    public Guid? TerminationReasonId { get; set; }

    /// <summary>
    /// Optional description for the termination reason.
    /// </summary>
    public string? TerminationReasonDescription { get; set; }

    /// <summary>
    /// The date when the contract is being terminated.
    /// </summary>
    public DateTime TerminationDate { get; set; }

    /// <summary>
    /// List of policies with their actual refund amounts.
    /// </summary>
    public List<TerminateContractPolicyItemDto> Policies { get; set; } = new();
}

/// <summary>
/// Policy item for contract termination.
/// </summary>
public class TerminateContractPolicyItemDto
{
    public Guid PolicyId { get; set; }

    /// <summary>
    /// Actual refund amount (editable, default = calculated refund).
    /// </summary>
    public decimal ActualRefundAmount { get; set; }
}
