using System;
using System.ComponentModel.DataAnnotations;
using iOne.PolicyContracts;

namespace iOne.Policy.PolicyContracts;

public class CreatePolicyContractDto
{
    [Required(ErrorMessage = "PolicyContract:InsurerIdRequired")]
    [Display(Name = "PolicyContract:InsurerId")]
    public Guid InsurerId { get; set; }

    [StringLength(50, ErrorMessage = "PolicyContract:InsurerContractCodeMaxLength")]
    [Display(Name = "PolicyContract:InsurerContractCode")]
    public string? InsurerContractCode { get; set; }

    [Display(Name = "PolicyContract:LobId")]
    public Guid? LobId { get; set; }

    [StringLength(50, ErrorMessage = "PolicyContract:CodeMaxLength")]
    [Display(Name = "PolicyContract:Code")]
    public string? Code { get; set; }

    [Required(ErrorMessage = "PolicyContract:NameRequired")]
    [StringLength(250, ErrorMessage = "PolicyContract:NameMaxLength")]
    [Display(Name = "PolicyContract:Name")]
    public string Name { get; set; } = null!;

    [Display(Name = "PolicyContract:Type")]
    public PolicyContractType Type { get; set; } = PolicyContractType.Group;

    [Required(ErrorMessage = "PolicyContract:CustomerIdRequired")]
    [Display(Name = "PolicyContract:CustomerId")]
    public Guid CustomerId { get; set; }

    [Display(Name = "PolicyContract:PayerId")]
    public Guid? PayerId { get; set; }

    [StringLength(250, ErrorMessage = "PolicyContract:PayerNameMaxLength")]
    [Display(Name = "PolicyContract:PayerName")]
    public string? PayerName { get; set; }

    [StringLength(50, ErrorMessage = "PolicyContract:PayerEmailMaxLength")]
    [Display(Name = "PolicyContract:PayerEmail")]
    public string? PayerEmail { get; set; }

    [StringLength(15, ErrorMessage = "PolicyContract:PayerPhoneMaxLength")]
    [Display(Name = "PolicyContract:PayerPhone")]
    public string? PayerPhone { get; set; }

    [Display(Name = "PolicyContract:PayerProvinceId")]
    public Guid? PayerProvinceId { get; set; }

    [Display(Name = "PolicyContract:PayerWardId")]
    public Guid? PayerWardId { get; set; }

    [StringLength(250, ErrorMessage = "PolicyContract:PayerAddressMaxLength")]
    [Display(Name = "PolicyContract:PayerAddress")]
    public string? PayerAddress { get; set; }

    [StringLength(500, ErrorMessage = "PolicyContract:PayerFullAddressMaxLength")]
    [Display(Name = "PolicyContract:PayerFullAddress")]
    public string? PayerFullAddress { get; set; }

    [StringLength(50, ErrorMessage = "PolicyContract:PayerTaxCodeMaxLength")]
    [Display(Name = "PolicyContract:PayerTaxCode")]
    public string? PayerTaxCode { get; set; }

    [Display(Name = "PolicyContract:EffectDate")]
    public DateTime? EffectDate { get; set; }

    [Display(Name = "PolicyContract:ExpireDate")]
    public DateTime? ExpireDate { get; set; }

    [Display(Name = "PolicyContract:Quantity")]
    public decimal Quantity { get; set; } = 1;

    [Display(Name = "PolicyContract:CurrentQuantity")]
    public decimal? CurrentQuantity { get; set; }

    [Display(Name = "PolicyContract:Status")]
    public PolicyContractStatus Status { get; set; } = PolicyContractStatus.Draft;

    [Display(Name = "PolicyContract:EmployeeId")]
    public Guid? EmployeeId { get; set; }

    [StringLength(500, ErrorMessage = "PolicyContract:DescriptionMaxLength")]
    [Display(Name = "PolicyContract:Description")]
    public string? Description { get; set; }

    [Display(Name = "PolicyContract:IsReciveInvoice")]
    public string IsReciveInvoice { get; set; } = "N";

    [Display(Name = "PolicyContract:DocumentId")]
    public Guid? DocumentId { get; set; }

    [Display(Name = "PolicyContract:CancellationDate")]
    public DateTime? CancellationDate { get; set; }

    [Display(Name = "PolicyContract:TerminationDate")]
    public DateTime? TerminationDate { get; set; }

    [Display(Name = "PolicyContract:CancellationReasonId")]
    public Guid? CancellationReasonId { get; set; }

    [Display(Name = "PolicyContract:TerminationReasonId")]
    public Guid? TerminationReasonId { get; set; }

    [StringLength(500, ErrorMessage = "PolicyContract:CancellationDescriptionMaxLength")]
    [Display(Name = "PolicyContract:CancellationDescription")]
    public string? CancellationDescription { get; set; }

    [StringLength(500, ErrorMessage = "PolicyContract:TerminationDescriptionMaxLength")]
    [Display(Name = "PolicyContract:TerminationDescription")]
    public string? TerminationDescription { get; set; }

    [Display(Name = "PolicyContract:QuotationId")]
    public Guid? QuotationId { get; set; }
}
