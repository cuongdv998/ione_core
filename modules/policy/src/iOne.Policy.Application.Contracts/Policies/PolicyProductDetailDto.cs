using System;
using System.Collections.Generic;

namespace iOne.Policy.Policies;

public class PolicyProductDetailDto
{
    public Guid Id { get; set; } // policy_product.id

    public Guid ProductId { get; set; }

    public decimal PremiumTotal { get; set; }
    public decimal Premium { get; set; }
    public decimal Vat { get; set; }
    public decimal? Discount { get; set; }
    public decimal? DiscountRate { get; set; }

    public decimal? Markup { get; set; }

    public decimal? AmountLiability { get; set; }
    public string? InsurerProductCode { get; set; }

    public List<PolicyCoverageDetailDto>? Coverages { get; set; }
}

