using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace iOne.Product.ProductPricing;

/// <summary>
/// Request DTO for calculating insurance product premiums.
/// </summary>
public class CalculatePremiumRequestDto
{
    /// <summary>
    /// Optional effective date for premium calculation. When both EffectiveDate and ExpireDate are provided,
    /// they are used for rate-by-time logic: if the period is exactly one year, no RATE_BY_TIME rule is applied;
    /// otherwise a product rule with RuleType RATE_BY_TIME may be executed to multiply premium by a time-based rate.
    /// </summary>
    public DateTime? EffectiveDate { get; set; }

    /// <summary>
    /// Optional expire date for premium calculation. When both EffectiveDate and ExpireDate are provided,
    /// they are used for rate-by-time logic (see EffectiveDate).
    /// </summary>
    public DateTime? ExpireDate { get; set; }

    /// <summary>
    /// The product ID (can be Guid or string, will be resolved).
    /// Note: The product code is passed in the URL path, this field is optional for additional validation.
    /// </summary>
    public Guid? ProductId { get; set; }

    /// <summary>
    /// Key-value pairs where keys match ProAttribute.Code (e.g., "carOld", "carPlate").
    /// Values can be strings or numbers depending on the attribute type.
    /// </summary>
    [Required]
    public Dictionary<string, object> Attributes { get; set; } = new();

    /// <summary>
    /// List of product coverages to calculate premiums for.
    /// </summary>
    [Required]
    public List<ProductCoveragePremiumRequestDto> ProductCoverages { get; set; } = new();
}
