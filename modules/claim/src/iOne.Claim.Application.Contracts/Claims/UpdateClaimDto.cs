using System;
using System.ComponentModel.DataAnnotations;
using iOne.Claims;

namespace iOne.Claim.Claims;

/// <summary>
/// DTO cập nhật yêu cầu bồi thường. Chỉ áp dụng cho hồ sơ trạng thái Nháp (Draft).
/// </summary>
public class UpdateClaimDto
{
    // Process Info
    [Required]
    public ProcessClaimType ProcessClaimType { get; set; }

    public Guid? ProcessDeptId { get; set; }

    public Guid? ProcessEmpId { get; set; }

    // Notifier Info
    [Required]
    [MaxLength(250)]
    public string NotifierName { get; set; } = null!;

    [Required]
    [MaxLength(15)]
    public string NotifierPhone { get; set; } = null!;

    [MaxLength(50)]
    [EmailAddress]
    public string? NotifierEmail { get; set; }

    [Required]
    [MaxLength(50)]
    public string NotifierInRelationship { get; set; } = null!;

    // Contact Info
    [Required]
    [MaxLength(250)]
    public string ContactName { get; set; } = null!;

    [Required]
    [MaxLength(15)]
    public string ContactPhone { get; set; } = null!;

    [MaxLength(50)]
    [EmailAddress]
    public string? ContactEmail { get; set; }

    [Required]
    [MaxLength(50)]
    public string ContactInRelationship { get; set; } = null!;

    // Incident Info
    [Required]
    public DateTime IncidentDate { get; set; }

    [MaxLength(50)]
    public string? CertificateNo { get; set; }

    [Required]
    public Guid InsurerId { get; set; }

    // Vehicle Info
    [MaxLength(15)]
    public string? CarPlate { get; set; }

    [MaxLength(25)]
    public string? Vin { get; set; }

    [MaxLength(25)]
    public string? EngineNumber { get; set; }

    // Location Info
    [Required]
    public Guid IncidentProvinceId { get; set; }

    [Required]
    public Guid IncidentWardId { get; set; }

    [MaxLength(250)]
    public string? IncidentAddress { get; set; }

    [Required]
    [MaxLength(1)]
    public string OnLocation { get; set; } = null!; // Y or N

    // Cause & Result
    [Required]
    public Guid IncidentCauseId { get; set; }

    [MaxLength(500)]
    public string? IncidentDescription { get; set; }

    [Required]
    [MaxLength(500)]
    public string IncidentResult { get; set; } = null!;

    public int? Priority { get; set; } // 1-3, default = 2

    // Assessment Info
    public DateTime? AssessmentDate { get; set; }
    public Guid? AssessmentPartnerId { get; set; }

    // Driver Info
    public int? PersonOnCar { get; set; }

    [MaxLength(250)]
    public string? DriverName { get; set; }

    [MaxLength(15)]
    public string? DriverPhone { get; set; }

    [MaxLength(1)]
    public string? DriverSex { get; set; } // M or F

    [MaxLength(15)]
    public string? DriverIdNo { get; set; }

    [MaxLength(50)]
    public string? DriverLicenseNo { get; set; }

    public DateTime? DriverLicenseEffectDate { get; set; }

    public DateTime? DriverLicenseExpireDate { get; set; }

    [MaxLength(5)]
    public string? DriverLicenseLevel { get; set; }

    [MaxLength(50)]
    public string? DriverRegistryNo { get; set; }

    public DateTime? DriverRegistryEffectDate { get; set; }

    public DateTime? DriverRegistryExpireDate { get; set; }

    // Other
    [MaxLength(500)]
    public string? Note { get; set; }
}
