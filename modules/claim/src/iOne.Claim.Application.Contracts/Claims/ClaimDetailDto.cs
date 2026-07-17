using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;
using iOne.Claims;

namespace iOne.Claim.Claims;

/// <summary>
/// DTO chi tiết yêu cầu bồi thường, dùng cho màn hình xem chi tiết.
/// Kết hợp thông tin từ Claim, ClaimIncident và ClaimIncidentRiskMotor.
/// </summary>
public class ClaimDetailDto : EntityDto<Guid>
{
    // Meta
    [Display(Name = "Claim:Code")]
    public string Code { get; set; } = null!;

    [Display(Name = "Claim:Status")]
    public ClaimStatus Status { get; set; }

    [Display(Name = "Claim:OpenDate")]
    public DateTime OpenDate { get; set; }

    [Display(Name = "Claim:NotifyDate")]
    public DateTime NotifyDate { get; set; }

    [Display(Name = "Claim:LobId")]
    public Guid LobId { get; set; }

    [Display(Name = "Claim:LobName")]
    public string? LobName { get; set; }

    [Display(Name = "Claim:InsurerId")]
    public Guid? InsurerId { get; set; }

    [Display(Name = "Claim:InsurerName")]
    public string? InsurerName { get; set; }

    [Display(Name = "Claim:SnapshotLink")]
    public string? SnapshotLink { get; set; }

    // Process Info
    [Display(Name = "Claim:ProcessClaimType")]
    public ProcessClaimType ProcessClaimType { get; set; }

    public Guid? ProcessDeptId { get; set; }

    public Guid? ProcessEmpId { get; set; }

    public string? ProcessDeptName { get; set; }

    public string? ProcessEmpName { get; set; }

    public string? ProcessEmpPhone { get; set; }

    public string? OpenEmployeePhone { get; set; }

    // Notifier Info
    public string NotifierName { get; set; } = null!;

    public string NotifierPhone { get; set; } = null!;

    public string? NotifierEmail { get; set; }

    public string? NotifierInRelationship { get; set; }

    // Contact Info
    public string ContactName { get; set; } = null!;

    public string ContactPhone { get; set; } = null!;

    public string? ContactEmail { get; set; }

    public string? ContactInRelationship { get; set; }

    // Incident Info
    public DateTime? IncidentDate { get; set; }

    public string? CertificateNo { get; set; }

    public Guid IncidentProvinceId { get; set; }

    public Guid IncidentWardId { get; set; }

    public string? IncidentAddress { get; set; }

    public string OnLocation { get; set; } = null!;

    public Guid IncidentCauseId { get; set; }

    public string? IncidentCauseName { get; set; }

    public string? IncidentDescription { get; set; }

    public string IncidentResult { get; set; } = null!;

    public int? Priority { get; set; }

    // Assessment Info
    public DateTime? AssessmentDate { get; set; }

    public Guid? AssessmentPartnerId { get; set; }

    public string? AssessmentPartnerName { get; set; }

    // Vehicle Info
    public string? CarPlate { get; set; }

    public string? Vin { get; set; }

    public string? EngineNumber { get; set; }

    public int? PersonOnCar { get; set; }

    // Driver Info
    public string? DriverName { get; set; }

    public string? DriverPhone { get; set; }

    public string? DriverSex { get; set; }

    public string? DriverIdNo { get; set; }

    public string? DriverLicenseNo { get; set; }

    public DateTime? DriverLicenseEffectDate { get; set; }

    public DateTime? DriverLicenseExpireDate { get; set; }

    public string? DriverLicenseLevel { get; set; }

    public string? DriverRegistryNo { get; set; }

    public DateTime? DriverRegistryEffectDate { get; set; }

    public DateTime? DriverRegistryExpireDate { get; set; }

    // Other
    public string? Note { get; set; }

    /// <summary>
    /// Tổng ước tổn thất: cộng dồn ước Active của các HSBT (ClaimFolder) không ở trạng thái Đã hủy.
    /// </summary>
    [Display(Name = "Claim:TotalEstimatedLossAmount")]
    public decimal TotalEstimatedLossAmount { get; set; }
}
