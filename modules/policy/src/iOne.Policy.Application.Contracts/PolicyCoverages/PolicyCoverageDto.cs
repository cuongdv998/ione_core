using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace iOne.Policy.PolicyCoverages;

public class PolicyCoverageDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "PolicyCoverage:PolicyProductId")]
    public Guid PolicyProductId { get; set; }

    [Display(Name = "PolicyCoverage:CoverageId")]
    public Guid CoverageId { get; set; }

    [Display(Name = "PolicyCoverage:CoverageParentId")]
    public Guid? CoverageParentId { get; set; }

    [Display(Name = "PolicyCoverage:InsurerCoverageCode")]
    public string? InsurerCoverageCode { get; set; }

    [Display(Name = "PolicyCoverage:UomId")]
    public Guid UomId { get; set; }

    [Display(Name = "PolicyCoverage:TableRateLineId")]
    public Guid? TableRateLineId { get; set; }

    [Display(Name = "PolicyCoverage:AmountLiability")]
    public decimal? AmountLiability { get; set; }

    [Display(Name = "PolicyCoverage:Quantity")]
    public decimal Quantity { get; set; }

    [Display(Name = "PolicyCoverage:TaxId")]
    public Guid TaxId { get; set; }

    [Display(Name = "PolicyCoverage:NetRate")]
    public decimal? NetRate { get; set; }

    [Display(Name = "PolicyCoverage:BaseRate")]
    public decimal? BaseRate { get; set; }

    [Display(Name = "PolicyCoverage:FlatRate")]
    public decimal? FlatRate { get; set; }

    [Display(Name = "PolicyCoverage:Loading")]
    public decimal? Loading { get; set; }

    [Display(Name = "PolicyCoverage:PremiumRate")]
    public decimal PremiumRate { get; set; }

    [Display(Name = "PolicyCoverage:PremiumTotal")]
    public decimal PremiumTotal { get; set; }

    [Display(Name = "PolicyCoverage:Premium")]
    public decimal Premium { get; set; }

    [Display(Name = "PolicyCoverage:Vat")]
    public decimal Vat { get; set; }

    [Display(Name = "PolicyCoverage:Discount")]
    public decimal? Discount { get; set; }

    [Display(Name = "PolicyCoverage:DiscountRate")]
    public decimal? DiscountRate { get; set; }
}
