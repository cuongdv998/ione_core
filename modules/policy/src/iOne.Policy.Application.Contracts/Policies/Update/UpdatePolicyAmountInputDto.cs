using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace iOne.Policy.Policies;

public class UpdatePolicyAmountInputDto
{
    [Display(Name = "PolicyAmount:FeeItemId")]
    public Guid? FeeItemId { get; set; }

    [Required(ErrorMessage = "Policy:PremiumTotalRequired")]
    [Display(Name = "Policy:PremiumTotal")]
    public decimal PremiumTotal { get; set; }

    [Required(ErrorMessage = "Policy:PremiumRequired")]
    [Display(Name = "Policy:Premium")]
    public decimal Premium { get; set; }

    [Required(ErrorMessage = "Policy:VatRequired")]
    [Display(Name = "Policy:Vat")]
    public decimal Vat { get; set; }

    [Display(Name = "Policy:Discount")]
    public decimal? Discount { get; set; }

    [Display(Name = "Policy:DiscountRate")]
    public decimal? DiscountRate { get; set; }

    [Display(Name = "Policy:Markup")]
    public decimal? Markup { get; set; }

    /// <summary>
    /// Endorsement net premium adjustment (preferred when saving PolicyAmount). Should match FE total.
    /// If null, BE uses sum of <see cref="EndorsementCoverageChanges"/> when present.
    /// </summary>
    [Display(Name = "Policy:EndorsementAdjustmentAmount")]
    public decimal? EndorsementAdjustmentAmount { get; set; }

    /// <summary>
    /// Per-coverage premium deltas when <see cref="EndorsementAdjustmentAmount"/> is not sent; BE aggregates into PolicyAmount.
    /// </summary>
    [Display(Name = "Policy:EndorsementCoverageChanges")]
    public List<EndorsementCoverageChangeDto>? EndorsementCoverageChanges { get; set; }
}
