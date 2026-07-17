using System;
using System.ComponentModel.DataAnnotations;

namespace iOne.Policy.PolicyAmounts;

public class UpdatePolicyAmountDto
{
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

    [Required(ErrorMessage = "PolicyAmount:PaymentStatusRequired")]
    [StringLength(15, ErrorMessage = "PolicyAmount:PaymentStatusMaxLength")]
    [Display(Name = "PolicyAmount:PaymentStatus")]
    public string PaymentStatus { get; set; } = null!;

    [Display(Name = "PolicyAmount:PaymentMethodId")]
    public Guid? PaymentMethodId { get; set; }

    [Display(Name = "PolicyAmount:PaymentDate")]
    public DateTime? PaymentDate { get; set; }
}
