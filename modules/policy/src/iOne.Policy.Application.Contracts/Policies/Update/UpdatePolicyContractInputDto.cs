using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using iOne.PolicyContracts;

namespace iOne.Policy.Policies;

public class UpdatePolicyContractInputDto
{
    public Guid? InsurerId { get; set; }

    [StringLength(50, ErrorMessage = "PolicyContract:InsurerContractCodeMaxLength")]
    [Display(Name = "PolicyContract:InsurerContractCode")]
    public string? InsurerContractCode { get; set; }

    [Display(Name = "PolicyContract:LobId")]
    public Guid? LobId { get; set; }

    // NOTE: contractNo (Code) and contractType (Type) are editable in policy edit mode.

    [StringLength(50, ErrorMessage = "PolicyContract:CodeMaxLength")]
    [Display(Name = "PolicyContract:Code")]
    public string? Code { get; set; }

    [Display(Name = "PolicyContract:Type")]
    public PolicyContractType? Type { get; set; }

    [StringLength(250, ErrorMessage = "PolicyContract:NameMaxLength")]
    [Display(Name = "PolicyContract:Name")]
    public string? Name { get; set; }

    public Guid? CustomerId { get; set; }

    [StringLength(50, ErrorMessage = "PolicyContract:PayerNameMaxLength")]
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

    [StringLength(250, ErrorMessage = "PolicyContract:PayerFullAddressMaxLength")]
    [Display(Name = "PolicyContract:PayerFullAddress")]
    public string? PayerFullAddress { get; set; }

    [StringLength(50, ErrorMessage = "PolicyContract:PayerTaxCodeMaxLength")]
    [Display(Name = "PolicyContract:PayerTaxCode")]
    public string? PayerTaxCode { get; set; }

    [StringLength(500, ErrorMessage = "PolicyContract:DescriptionMaxLength")]
    [Display(Name = "PolicyContract:Description")]
    public string? Description { get; set; }

    [Display(Name = "PolicyContract:EffectDate")]
    public DateTime? EffectDate { get; set; }

    [Display(Name = "PolicyContract:ExpireDate")]
    public DateTime? ExpireDate { get; set; }

    [Display(Name = "PolicyContract:Quantity")]
    public decimal? Quantity { get; set; }

    [Display(Name = "PolicyContract:CurrentQuantity")]
    public decimal? CurrentQuantity { get; set; }

    [Display(Name = "PolicyContract:EmployeeId")]
    public Guid? EmployeeId { get; set; }

    [StringLength(1, ErrorMessage = "PolicyContract:IsReciveInvoiceMaxLength")]
    [Display(Name = "PolicyContract:IsReciveInvoice")]
    public string? IsReciveInvoice { get; set; }

    public List<UpdatePolicyContractDocumentInputDto>? Documents { get; set; }
}

public class UpdatePolicyContractDocumentInputDto
{
    public Guid DocumentId { get; set; }
}
