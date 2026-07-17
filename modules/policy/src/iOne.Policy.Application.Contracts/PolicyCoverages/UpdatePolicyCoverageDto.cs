using System;
using System.ComponentModel.DataAnnotations;

namespace iOne.Policy.PolicyCoverages;

public class UpdatePolicyCoverageDto
{
    [Display(Name = "PolicyCoverage:CoverageParentId")]
    public Guid? CoverageParentId { get; set; }

    [StringLength(50, ErrorMessage = "PolicyCoverage:InsurerCoverageCodeMaxLength")]
    [Display(Name = "PolicyCoverage:InsurerCoverageCode")]
    public string? InsurerCoverageCode { get; set; }

    [Display(Name = "PolicyCoverage:TableRateLineId")]
    public Guid? TableRateLineId { get; set; }

    [Display(Name = "PolicyCoverage:AmountLiability")]
    public decimal? AmountLiability { get; set; }

    [Required(ErrorMessage = "PolicyCoverage:QuantityRequired")]
    [Display(Name = "PolicyCoverage:Quantity")]
    public decimal Quantity { get; set; }

    [Display(Name = "PolicyCoverage:NetRate")]
    public decimal? NetRate { get; set; }

    [Display(Name = "PolicyCoverage:BaseRate")]
    public decimal? BaseRate { get; set; }

    [Display(Name = "PolicyCoverage:FlatRate")]
    public decimal? FlatRate { get; set; }

    [Display(Name = "PolicyCoverage:Loading")]
    public decimal? Loading { get; set; }

    [Required(ErrorMessage = "PolicyCoverage:PremiumRateRequired")]
    [Display(Name = "PolicyCoverage:PremiumRate")]
    public decimal PremiumRate { get; set; }

    [Required(ErrorMessage = "PolicyCoverage:PremiumTotalRequired")]
    [Display(Name = "PolicyCoverage:PremiumTotal")]
    public decimal PremiumTotal { get; set; }

    [Required(ErrorMessage = "PolicyCoverage:PremiumRequired")]
    [Display(Name = "PolicyCoverage:Premium")]
    public decimal Premium { get; set; }

    [Required(ErrorMessage = "PolicyCoverage:VatRequired")]
    [Display(Name = "PolicyCoverage:Vat")]
    public decimal Vat { get; set; }

    [Display(Name = "PolicyCoverage:Discount")]
    public decimal? Discount { get; set; }

    [Display(Name = "PolicyCoverage:DiscountRate")]
    public decimal? DiscountRate { get; set; }
}
