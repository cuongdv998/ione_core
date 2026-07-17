using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace iOne.Policy.Policies;

public class UpdatePolicyProductInputDto
{
    // policy_product.id (nullable: null means create new row)
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "PolicyProduct:ProductIdRequired")]
    [Display(Name = "PolicyProduct:ProductId")]
    public Guid ProductId { get; set; }

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

    // Product attributes with isRequired from by-lob-partner API (for validation: isRequired=Y => value must be present)
    public List<PolicyProductAttributeInputDto>? Attributes { get; set; }

    // Nested coverages for this product (policy_coverage + policy_coverage_level)
    public List<UpdatePolicyCoverageInputDto>? Coverages { get; set; }
}

