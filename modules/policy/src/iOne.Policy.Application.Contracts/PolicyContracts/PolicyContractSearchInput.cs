using System;
using iOne.PolicyContracts;
using Volo.Abp.Application.Dtos;

namespace iOne.Policy.PolicyContracts;

public class PolicyContractSearchInput : PagedAndSortedResultRequestDto
{
    public Guid? CustomerId { get; set; }
    public string? Code { get; set; }
    public PolicyContractType? Type { get; set; }
    public PolicyContractStatus? Status { get; set; }
    public string? CertificateCode { get; set; }
    public string? PolicyNo { get; set; }
    public Guid? SellerId { get; set; }
    public DateTime? EffectDateFrom { get; set; }
    public DateTime? EffectDateTo { get; set; }
    public DateTime? ExpireDateFrom { get; set; }
    public DateTime? ExpireDateTo { get; set; }
    public Guid? InsurerId { get; set; }
}
