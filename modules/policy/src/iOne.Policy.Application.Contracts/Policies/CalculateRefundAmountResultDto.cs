using System;
using System.Collections.Generic;

namespace iOne.Policy.Policies;

/// <summary>
/// Result DTO for refund amount calculation.
/// </summary>
public class CalculateRefundAmountResultDto
{
    /// <summary>
    /// Total refund amount (negative value indicates money to be refunded to customer).
    /// </summary>
    public decimal TotalRefund { get; set; }

    /// <summary>
    /// Refund amounts distributed to each coverage.
    /// </summary>
    public List<CoverageRefundDto> CoverageRefunds { get; set; } = new();
}

/// <summary>
/// Refund amount for a specific coverage.
/// </summary>
public class CoverageRefundDto
{
    /// <summary>
    /// Coverage ID.
    /// </summary>
    public Guid CoverageId { get; set; }

    /// <summary>
    /// Refund amount for this coverage (negative value).
    /// </summary>
    public decimal RefundAmount { get; set; }
}
