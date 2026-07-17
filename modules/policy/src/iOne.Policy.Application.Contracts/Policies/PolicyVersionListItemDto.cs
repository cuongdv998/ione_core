using System;
using System.ComponentModel.DataAnnotations;

namespace iOne.Policy.Policies;

/// <summary>
/// Summary of a policy version for list display (e.g. in policy list Versions array).
/// </summary>
public class PolicyVersionListItemDto
{
    public Guid Id { get; set; }

    [Display(Name = "Policy:Version")]
    public int Version { get; set; }

    public string? Type { get; set; }
    public string? Status { get; set; }
    public string? ApprovalStatus { get; set; }

    public DateTime EffectDate { get; set; }
    public DateTime ExpireDate { get; set; }

    public decimal PremiumTotal { get; set; }

    [Display(Name = "Policy:CertificateNo")]
    public string? CertificateNo { get; set; }

    [Display(Name = "Policy:ProductName")]
    public string? ProductName { get; set; }

    [Display(Name = "Policy:VehiclePlate")]
    public string? VehiclePlate { get; set; }

    [Display(Name = "Policy:ChassisNumber")]
    public string? ChassisNumber { get; set; }

    [Display(Name = "Policy:EngineNumber")]
    public string? EngineNumber { get; set; }
}
