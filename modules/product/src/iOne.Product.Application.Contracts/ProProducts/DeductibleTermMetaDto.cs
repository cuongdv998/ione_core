using System;

namespace iOne.Product.ProProducts;

/// <summary>
/// Metadata for a deductible term (used when saving policy_coverage_level).
/// </summary>
public class DeductibleTermMetaDto
{
    public Guid CoverageLevelTypeId { get; set; }
    public Guid CoverageLevelBasisId { get; set; }
    public string AmountType { get; set; } = null!;
    public decimal FromAmount { get; set; }
    public decimal ToAmount { get; set; }
    public string? ConditionScript { get; set; }
    public string? ComputeScript { get; set; }
}
