using System;
using System.ComponentModel.DataAnnotations;

namespace iOne.Product.ProductPricing;

/// <summary>
/// Request DTO for a single product coverage in premium calculation.
/// </summary>
public class ProductCoveragePremiumRequestDto
{
    /// <summary>
    /// The ProProductCoverage ID.
    /// </summary>
    [Required]
    public Guid ProductCoverageId { get; set; }

    /// <summary>
    /// The ProCoverage ID.
    /// </summary>
    [Required]
    public Guid CoverageId { get; set; }

    /// <summary>
    /// ProCoverage.Code – gửi kèm để hỗ trợ tra cứu/debug bên pricing.
    /// </summary>
    public string? CoverageCode { get; set; }

    /// <summary>
    /// The insured amount used for premium calculation.
    /// </summary>
    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "AmountLiability must be a positive number.")]
    public decimal AmountLiability { get; set; }

    /// <summary>
    /// Quantity used to scale the calculated premium (default 1).
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
    public int Quantity { get; set; } = 1;

    /// <summary>
    /// Mức miễn thường (deductible) – term id hoặc giá trị đã chọn, cùng cấp với CoverageCode, AmountLiability. Dùng trong rule/script tính phí.
    /// </summary>
    public string? Deductible { get; set; }
}
