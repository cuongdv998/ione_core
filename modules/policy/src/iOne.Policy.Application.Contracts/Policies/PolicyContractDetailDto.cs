using System;
using System.ComponentModel.DataAnnotations;
using iOne.PolicyContracts;

namespace iOne.Policy.Policies;

public class PolicyContractDetailDto
{
    public Guid? Id { get; set; }

    public Guid? InsurerId { get; set; }

    [StringLength(50)]
    public string? InsurerContractCode { get; set; }

    public Guid? LobId { get; set; }

    [StringLength(50)]
    public string? Code { get; set; }

    [StringLength(250)]
    public string? Name { get; set; }

    public PolicyContractType Type { get; set; }

    public Guid CustomerId { get; set; }

    [StringLength(250)]
    public string? PayerName { get; set; }

    [StringLength(50)]
    public string? PayerEmail { get; set; }

    [StringLength(15)]
    public string? PayerPhone { get; set; }

    public Guid? PayerProvinceId { get; set; }
    public Guid? PayerWardId { get; set; }

    [StringLength(250)]
    public string? PayerAddress { get; set; }

    [StringLength(500)]
    public string? PayerFullAddress { get; set; }

    [StringLength(50)]
    public string? PayerTaxCode { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    public DateTime EffectDate { get; set; }
    public DateTime? ExpireDate { get; set; }

    public decimal? Quantity { get; set; }
    public decimal? CurrentQuantity { get; set; }

    public Guid? EmployeeId { get; set; }

    [StringLength(1)]
    public string IsReciveInvoice { get; set; } = "N";

    public System.Collections.Generic.List<PolicyContractDocumentDetailDto>? Documents { get; set; }
}

