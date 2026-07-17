using System;
using System.ComponentModel.DataAnnotations;
using iOne.PolicyContracts;
using Volo.Abp.Application.Dtos;

namespace iOne.Policy.PolicyContracts;

public class PolicyContractDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "PolicyContract:InsurerId")]
    public Guid? InsurerId { get; set; }

    [Display(Name = "PolicyContract:InsurerContractCode")]
    public string? InsurerContractCode { get; set; }

    [Display(Name = "PolicyContract:LobId")]
    public Guid? LobId { get; set; }

    [Display(Name = "PolicyContract:Code")]
    public string Code { get; set; } = null!;

    [Display(Name = "PolicyContract:Name")]
    public string Name { get; set; } = null!;

    [Display(Name = "PolicyContract:Type")]
    public PolicyContractType Type { get; set; }

    [Display(Name = "PolicyContract:CustomerId")]
    public Guid CustomerId { get; set; }

    [Display(Name = "PolicyContract:PayerName")]
    public string? PayerName { get; set; }

    [Display(Name = "PolicyContract:PayerEmail")]
    public string? PayerEmail { get; set; }

    [Display(Name = "PolicyContract:PayerPhone")]
    public string? PayerPhone { get; set; }

    [Display(Name = "PolicyContract:PayerProvinceId")]
    public Guid? PayerProvinceId { get; set; }

    [Display(Name = "PolicyContract:PayerWardId")]
    public Guid? PayerWardId { get; set; }

    [Display(Name = "PolicyContract:PayerAddress")]
    public string? PayerAddress { get; set; }

    [Display(Name = "PolicyContract:PayerFullAddress")]
    public string? PayerFullAddress { get; set; }

    [Display(Name = "PolicyContract:PayerTaxCode")]
    public string? PayerTaxCode { get; set; }

    [Display(Name = "PolicyContract:Description")]
    public string? Description { get; set; }

    [Display(Name = "PolicyContract:EffectDate")]
    public DateTime EffectDate { get; set; }

    [Display(Name = "PolicyContract:ExpireDate")]
    public DateTime? ExpireDate { get; set; }

    [Display(Name = "PolicyContract:Quantity")]
    public decimal Quantity { get; set; }

    [Display(Name = "PolicyContract:CurrentQuantity")]
    public decimal? CurrentQuantity { get; set; }

    [Display(Name = "PolicyContract:Status")]
    public PolicyContractStatus Status { get; set; }

    [Display(Name = "PolicyContract:EmployeeId")]
    public Guid? EmployeeId { get; set; }

    [Display(Name = "PolicyContract:CancellationDate")]
    public DateTime? CancellationDate { get; set; }

    [Display(Name = "PolicyContract:TerminationDate")]
    public DateTime? TerminationDate { get; set; }

    [Display(Name = "PolicyContract:CancellationReasonId")]
    public Guid? CancellationReasonId { get; set; }

    [Display(Name = "PolicyContract:TerminationReasonId")]
    public Guid? TerminationReasonId { get; set; }

    [Display(Name = "PolicyContract:CancellationDescription")]
    public string? CancellationDescription { get; set; }

    [Display(Name = "PolicyContract:TerminationDescription")]
    public string? TerminationDescription { get; set; }

    [Display(Name = "PolicyContract:IsReciveInvoice")]
    public string IsReciveInvoice { get; set; } = "N";

    [Display(Name = "PolicyContract:QuotationId")]
    public Guid? QuotationId { get; set; }

    [Display(Name = "PolicyContract:DocumentId")]
    public Guid? DocumentId { get; set; }
}
