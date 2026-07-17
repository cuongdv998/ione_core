using System;
using System.Collections.Generic;

namespace iOne.Claim.Claims;

public class PolicyDto
{
    public Guid PolicyId { get; set; }

    public string LobName { get; set; } = null!;

    public string ContractId { get; set; } = null!;

    public string? CertificateNo { get; set; }

    public List<string> Products { get; set; } = new List<string>();

    public string? CarPlate { get; set; }

    public string OwnerName { get; set; } = null!;

    public DateTime EffectDate { get; set; }

    public DateTime ExpireDate { get; set; }

    public string Status { get; set; } = null!;

    public string? CertificateUrl { get; set; }

    /// <summary>Đối tác bảo hiểm (công ty bảo hiểm) của đơn - từ Policy.Contract.InsurerId.</summary>
    public Guid? InsurerId { get; set; }

    /// <summary>Tên đối tác bảo hiểm - từ Policy.Contract.Insurer.Name.</summary>
    public string? InsurerName { get; set; }
}
