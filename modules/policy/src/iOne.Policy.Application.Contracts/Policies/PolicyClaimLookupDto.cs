using System;
using System.Collections.Generic;

namespace iOne.Policy.Policies;

public class PolicyClaimLookupDto
{
    public Guid PolicyId { get; set; }

    public string? LobName { get; set; }

    /// <summary>
    /// Business contract code (PolicyContract.Code), used by UI as "ContractId".
    /// </summary>
    public string? ContractId { get; set; }

    public string? CertificateNo { get; set; }

    public List<string> Products { get; set; } = new();

    /// <summary>
    /// ProProduct id for this lookup row (one row per policy product when Products is non-empty).
    /// </summary>
    public Guid? ProductId { get; set; }

    public string? CarPlate { get; set; }

    public string? OwnerName { get; set; }

    public DateTime EffectDate { get; set; }

    public DateTime ExpireDate { get; set; }

    public string? Status { get; set; }

    /// <summary>
    /// Payment status from policy_amount.payment_status (new, paid, partial, cancelled, ...)
    /// </summary>
    public string? PaymentStatus { get; set; }

    public string? CertificateUrl { get; set; }

    /// <summary>
    /// Insurer (công ty bảo hiểm) của đơn - từ Policy.Contract.InsurerId.
    /// Dùng để lấy danh sách InsurerName từ kết quả tìm kiếm và lọc đơn theo insurer.
    /// </summary>
    public Guid? InsurerId { get; set; }

    public string? InsurerName { get; set; }
}

