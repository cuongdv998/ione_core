using System;
using System.Collections.Generic;

namespace iOne.Policy.Policies;

/// <summary>
/// Input DTO for calculating refund amounts when terminating a policy.
/// </summary>
public class CalculateRefundAmountInput
{
    /// <summary>
    /// Product code (e.g., "DBV_VCX").
    /// </summary>
    public string ProductId { get; set; } = null!;

    /// <summary>
    /// Policy attributes as key-value pairs (e.g., carPlate, carAge, carGroup, etc.).
    /// </summary>
    public Dictionary<string, object?> Attributes { get; set; } = new();

    /// <summary>
    /// The policy ID being terminated.
    /// </summary>
    public Guid PolicyId { get; set; }

    /// <summary>
    /// Policy effective date.
    /// </summary>
    public DateTime PolicyEffectDate { get; set; }

    /// <summary>
    /// Policy expiration date.
    /// </summary>
    public DateTime PolicyExpireDate { get; set; }

    /// <summary>
    /// The date when the policy is being terminated.
    /// </summary>
    public DateTime TerminationDate { get; set; }

    /// <summary>
    /// List of product coverages with their premium information.
    /// </summary>
    public List<CalculateRefundCoverageInput> ProductCoverages { get; set; } = new();

    /// <summary>
    /// Total premium amount (excluding VAT).
    /// </summary>
    public decimal TotalPremium { get; set; }

    /// <summary>
    /// Total VAT amount.
    /// </summary>
    public decimal TotalPremiumVAT { get; set; }
}

/// <summary>
/// Batch input DTO for calculating refund amounts across multiple products when terminating a policy.
/// </summary>
public class CalculateRefundAmountBatchInput
{
    /// <summary>
    /// The policy ID being terminated.
    /// </summary>
    public Guid PolicyId { get; set; }

    /// <summary>
    /// The date when the policy is being terminated.
    /// </summary>
    public DateTime TerminationDate { get; set; }

    /// <summary>
    /// Policy attributes as key-value pairs (shared across all products, e.g., carPlate, carAge, etc.).
    /// </summary>
    public Dictionary<string, object?> Attributes { get; set; } = new();

    /// <summary>
    /// List of per-product refund inputs.
    /// </summary>
    public List<ProductRefundInput> Products { get; set; } = new();
}

/// <summary>
/// Per-product input for batch refund calculation.
/// </summary>
public class ProductRefundInput
{
    /// <summary>
    /// Product code (e.g., "DBV_VCX").
    /// </summary>
    public string ProductId { get; set; } = null!;

    /// <summary>
    /// List of coverages belonging to this product.
    /// </summary>
    public List<CalculateRefundCoverageInput> ProductCoverages { get; set; } = new();

    /// <summary>
    /// Total premium amount for this product (excluding VAT).
    /// </summary>
    public decimal TotalPremium { get; set; }

    /// <summary>
    /// Total VAT amount for this product.
    /// </summary>
    public decimal TotalPremiumVAT { get; set; }
}

/// <summary>
/// Coverage information for refund calculation.
/// </summary>
public class CalculateRefundCoverageInput
{
    /// <summary>
    /// Product coverage ID.
    /// </summary>
    public string? ProductCoverageId { get; set; }

    /// <summary>
    /// Coverage ID.
    /// </summary>
    public Guid CoverageId { get; set; }

    /// <summary>
    /// Liability amount.
    /// </summary>
    public decimal? AmountLiability { get; set; }

    /// <summary>
    /// Quantity.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Premium amount (excluding VAT).
    /// </summary>
    public decimal Premium { get; set; }

    /// <summary>
    /// VAT amount.
    /// </summary>
    public decimal PremiumVAT { get; set; }
}
