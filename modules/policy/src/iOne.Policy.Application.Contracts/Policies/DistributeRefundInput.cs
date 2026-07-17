using System.Collections.Generic;

namespace iOne.Policy.Policies;

/// <summary>
/// Input for distributing a total refund amount across coverages by premium ratio.
/// </summary>
public class DistributeRefundInput
{
    /// <summary>
    /// List of coverages with premium and premiumVAT for each.
    /// </summary>
    public List<CalculateRefundCoverageInput> Coverages { get; set; } = new();

    /// <summary>
    /// Total refund amount (typically negative) to distribute.
    /// </summary>
    public decimal TotalRefund { get; set; }
}
