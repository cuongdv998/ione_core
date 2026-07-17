using System;
using System.Collections.Generic;

namespace iOne.Policy.Policies;

public class PolicyCoverageDetailDto
{
    public Guid Id { get; set; } // policy_coverage.id

    public Guid CoverageId { get; set; }
    public Guid? CoverageParentId { get; set; }
    public Guid? UomId { get; set; }
    public Guid? TaxId { get; set; }

    public decimal Quantity { get; set; }
    public decimal PremiumRate { get; set; }
    public decimal PremiumTotal { get; set; }
    public decimal Premium { get; set; }
    public decimal Vat { get; set; }

    public string? InsurerCoverageCode { get; set; }
    public Guid? TableRateLineId { get; set; }
    public decimal? AmountLiability { get; set; }

    public decimal? NetRate { get; set; }
    public decimal? BaseRate { get; set; }
    public decimal? FlatRate { get; set; }
    public decimal? Loading { get; set; }
    public decimal? Discount { get; set; }
    public decimal? DiscountRate { get; set; }

    /// <summary>
    /// Per-coverage endorsement adjustment (tăng/giảm phí). Filled by backend when loading an endorsement version:
    /// (current Premium + Vat) − (previous version same coverage Premium + Vat). Null when not an endorsement or no previous version.
    /// </summary>
    public decimal? PremiumChange { get; set; }

    public List<PolicyCoverageLevelDetailDto>? CoverageLevels { get; set; }
}

