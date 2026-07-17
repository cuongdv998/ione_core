using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace iOne.Policy.PolicyProducts;

public class PolicyProductDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "PolicyProduct:PolicyVersionId")]
    public Guid PolicyVersionId { get; set; }

    [Display(Name = "PolicyProduct:ProductId")]
    public Guid ProductId { get; set; }

    [Display(Name = "PolicyProduct:InsurerProductCode")]
    public string? InsurerProductCode { get; set; }

    [Display(Name = "PolicyProduct:AmountLiability")]
    public decimal? AmountLiability { get; set; }

    [Display(Name = "PolicyProduct:PremiumTotal")]
    public decimal PremiumTotal { get; set; }

    [Display(Name = "PolicyProduct:Premium")]
    public decimal Premium { get; set; }

    [Display(Name = "PolicyProduct:Vat")]
    public decimal Vat { get; set; }

    [Display(Name = "PolicyProduct:Discount")]
    public decimal? Discount { get; set; }

    [Display(Name = "PolicyProduct:DiscountRate")]
    public decimal? DiscountRate { get; set; }

    [Display(Name = "PolicyProduct:Markup")]
    public decimal? Markup { get; set; }
}
