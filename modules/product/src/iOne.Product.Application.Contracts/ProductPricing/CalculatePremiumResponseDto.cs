using System;
using System.Collections.Generic;

namespace iOne.Product.ProductPricing;

/// <summary>
/// Response DTO for premium calculation containing all coverage premiums.
/// </summary>
public class CalculatePremiumResponseDto
{
    /// <summary>
    /// The product ID.
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// The attributes that were used for calculation (echoed from request).
    /// </summary>
    public Dictionary<string, object> Attributes { get; set; } = new();

    /// <summary>
    /// List of product coverages with calculated premiums.
    /// Note: Coverages without matching rates are omitted from this list.
    /// </summary>
    public List<ProductCoveragePremiumResponseDto> ProductCoverages { get; set; } = new();
}
