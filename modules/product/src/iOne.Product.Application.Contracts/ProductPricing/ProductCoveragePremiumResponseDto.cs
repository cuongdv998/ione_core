using System;

namespace iOne.Product.ProductPricing;

/// <summary>
/// Response DTO for a single product coverage with calculated premium.
/// </summary>
public class ProductCoveragePremiumResponseDto
{
    /// <summary>
    /// The ProProductCoverage ID.
    /// </summary>
    public Guid ProductCoverageId { get; set; }

    /// <summary>
    /// The ProCoverage ID.
    /// </summary>
    public Guid CoverageId { get; set; }

    /// <summary>
    /// The matched rate information.
    /// </summary>
    public MatchedRateDto Rate { get; set; } = null!;

    /// <summary>
    /// The calculated premium amount.
    /// </summary>
    public decimal Premium { get; set; }

    /// <summary>
    /// The VAT amount on premium.
    /// </summary>
    public decimal? PremiumVat { get; set; }
    
    /// <summary>
    /// The raw VAT amount on premium.
    /// </summary>
    public decimal? Vat { get; set; }
}
