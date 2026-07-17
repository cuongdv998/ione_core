using System.ComponentModel.DataAnnotations;

namespace iOne.Policy.PolicyProducts;

public class UpdatePolicyProductDto
{
    [StringLength(50, ErrorMessage = "PolicyProduct:InsurerProductCodeMaxLength")]
    [Display(Name = "PolicyProduct:InsurerProductCode")]
    public string? InsurerProductCode { get; set; }

    [Display(Name = "PolicyProduct:AmountLiability")]
    public decimal? AmountLiability { get; set; }

    [Required(ErrorMessage = "PolicyProduct:PremiumTotalRequired")]
    [Display(Name = "PolicyProduct:PremiumTotal")]
    public decimal PremiumTotal { get; set; }

    [Required(ErrorMessage = "PolicyProduct:PremiumRequired")]
    [Display(Name = "PolicyProduct:Premium")]
    public decimal Premium { get; set; }

    [Required(ErrorMessage = "PolicyProduct:VatRequired")]
    [Display(Name = "PolicyProduct:Vat")]
    public decimal Vat { get; set; }

    [Display(Name = "PolicyProduct:Discount")]
    public decimal? Discount { get; set; }

    [Display(Name = "PolicyProduct:DiscountRate")]
    public decimal? DiscountRate { get; set; }

    [Display(Name = "PolicyProduct:Markup")]
    public decimal? Markup { get; set; }
}
