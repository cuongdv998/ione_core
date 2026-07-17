using System;

namespace iOne.Product.ProductPricing;

/// <summary>
/// DTO representing the matched rate information from ProTableRateLine.
/// </summary>
public class MatchedRateDto
{
    /// <summary>
    /// The ID of the matched ProTableRateLine.
    /// </summary>
    public Guid RateId { get; set; }

    /// <summary>
    /// The base rate as percentage (0-100) used for calculation.
    /// PremiumVAT (before reverse) = (BaseRate/100) * AmountLiability when this rate is used.
    /// </summary>
    public decimal? BaseRate { get; set; }

    /// <summary>
    /// The flat rate (fixed amount) used when BaseRate is null.
    /// </summary>
    public decimal? FlatRate { get; set; }

    /// <summary>
    /// Phí bổ sung (loading) từ rate line; khi khác null và khác 0 đã được cộng vào BaseRate/FlatRate khi tính phí.
    /// </summary>
    public decimal? Loading { get; set; }

    /// <summary>
    /// The tax rate percentage (0-100) applied to this coverage.
    /// </summary>
    public decimal? TaxValue { get; set; }
}
