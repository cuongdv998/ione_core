using System;
using System.ComponentModel.DataAnnotations;

namespace iOne.Policy.PolicyAmounts;

public class CreatePolicyAmountDto
{
    [Required(ErrorMessage = "PolicyAmount:PolicyIdRequired")]
    [Display(Name = "PolicyAmount:PolicyId")]
    public Guid PolicyId { get; set; }

    [Required(ErrorMessage = "PolicyAmount:PolicyVersionIdRequired")]
    [Display(Name = "PolicyAmount:PolicyVersionId")]
    public Guid PolicyVersionId { get; set; }

    [Required(ErrorMessage = "PolicyAmount:FeeItemIdRequired")]
    [Display(Name = "PolicyAmount:FeeItemId")]
    public Guid FeeItemId { get; set; }

    [Required(ErrorMessage = "PolicyAmount:IssueDateRequired")]
    [Display(Name = "PolicyAmount:IssueDate")]
    public DateTime IssueDate { get; set; }

    [Required(ErrorMessage = "PolicyAmount:AmountTotalRequired")]
    [Display(Name = "PolicyAmount:AmountTotal")]
    public decimal AmountTotal { get; set; }

    [Required(ErrorMessage = "PolicyAmount:AmountRequired")]
    [Display(Name = "PolicyAmount:Amount")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "PolicyAmount:VatRequired")]
    [Display(Name = "PolicyAmount:Vat")]
    public decimal Vat { get; set; }

    [StringLength(15, ErrorMessage = "PolicyAmount:PaymentStatusMaxLength")]
    [Display(Name = "PolicyAmount:PaymentStatus")]
    public string PaymentStatus { get; set; } = "new";

    [Display(Name = "PolicyAmount:PaymentMethodId")]
    public Guid? PaymentMethodId { get; set; }

    [Display(Name = "PolicyAmount:PaymentDate")]
    public DateTime? PaymentDate { get; set; }
}
