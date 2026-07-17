using System;
using iOne.Policies;
using Volo.Abp.Application.Dtos;

namespace iOne.Policy.Policies;

public class GetPoliciesInput : PagedAndSortedResultRequestDto
{
    public string? PolicyNo { get; set; }

    public Guid? ContractId { get; set; }

    public Guid? LobId { get; set; }

    public Guid? PolicyTypeId { get; set; }

    public Guid? PartnerId { get; set; }

    public Guid? SellerId { get; set; }

    public Guid? ImplementerId { get; set; }

    public Guid? CurrencyId { get; set; }

    public PolicySellType? SellType { get; set; }

    public PolicyStatus? Status { get; set; }

    public string? ApprovalStatus { get; set; }

    // New filter properties for wireframe implementation
    public Guid? ChannelId { get; set; }

    public Guid? CustomerId { get; set; }

    public string? ContractType { get; set; }

    public string? ContractStatus { get; set; }

    public string? CertificateNo { get; set; }

    public Guid? PolicyIssuerId { get; set; }

    public DateTime? EffectiveDateFrom { get; set; }

    public DateTime? EffectiveDateTo { get; set; }

    public DateTime? ExpiryDateFrom { get; set; }

    public DateTime? ExpiryDateTo { get; set; }

    // Additional search filters
    public string? CarPlate { get; set; }

    public string? CarVin { get; set; }

    public string? CarEngineNumber { get; set; }

    public Guid? PrimaryInsurancePartnerId { get; set; }

    public string? ImportLotNumber { get; set; }

    /// <summary>
    /// Lọc theo <c>policy_amount.payment_status</c> của phiên bản: <c>new</c>, <c>paid</c>, <c>partial</c>.
    /// </summary>
    public string? PaymentStatus { get; set; }
}
