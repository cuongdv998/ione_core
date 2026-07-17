using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace iOne.Policy.PolicyAmounts;

public class PolicyAmountDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "PolicyAmount:PolicyId")]
    public Guid PolicyId { get; set; }

    [Display(Name = "PolicyAmount:PolicyVersionId")]
    public Guid PolicyVersionId { get; set; }

    [Display(Name = "PolicyAmount:FeeItemId")]
    public Guid FeeItemId { get; set; }

    [Display(Name = "PolicyAmount:IssueDate")]
    public DateTime IssueDate { get; set; }

    [Display(Name = "PolicyAmount:AmountTotal")]
    public decimal AmountTotal { get; set; }

    [Display(Name = "PolicyAmount:Amount")]
    public decimal Amount { get; set; }

    [Display(Name = "PolicyAmount:Vat")]
    public decimal Vat { get; set; }

    [Display(Name = "PolicyAmount:PaymentStatus")]
    public string PaymentStatus { get; set; } = null!;

    [Display(Name = "PolicyAmount:PaymentMethodId")]
    public Guid? PaymentMethodId { get; set; }

    [Display(Name = "PolicyAmount:PaymentDate")]
    public DateTime? PaymentDate { get; set; }
}
