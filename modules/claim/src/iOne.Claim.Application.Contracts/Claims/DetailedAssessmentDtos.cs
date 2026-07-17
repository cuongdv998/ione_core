using System;
using System.Collections.Generic;
using iOne.WorkTasks;

namespace iOne.Claim.Claims;

public class DetailedAssessmentOptionDto
{
    public string Id { get; set; } = null!;
    public string? Code { get; set; }
    public string Name { get; set; } = null!;
    public string? ObjectTypeId { get; set; }
    public string? ObjectTypeCode { get; set; }
    public string? ObjectTypeName { get; set; }
    public string? ObjectTypeGroup { get; set; }
    public string? ObjectKind { get; set; }
    public string? Type { get; set; }
}

public class DetailedAssessmentAttachmentDto
{
    public string Id { get; set; } = null!;
    public string? ClaimDocumentId { get; set; }
    public string? DocumentId { get; set; }
    public string FileName { get; set; } = null!;
    public string? Url { get; set; }
}

public class DetailedAssessmentItemDto
{
    public string Id { get; set; } = null!;
    public string? ItemId { get; set; }
    public string? ItemName { get; set; }
    public string? PersonName { get; set; }
    public string? PersonIdNo { get; set; }
    public string? Description { get; set; }
    public decimal? Quantity { get; set; }
    public string? RiskId { get; set; }
    public string? ClaimPlanId { get; set; }
    public string? IsGenuine { get; set; }
    public bool IsRecovery { get; set; }
    public DateTime? DischargeDate { get; set; }
    public DateTime? IssueDate { get; set; }
    public List<DetailedAssessmentAttachmentDto> Attachments { get; set; } = new();
}

public class DetailedAssessmentSectionDto
{
    public string Id { get; set; } = null!;
    public string CoverageId { get; set; } = null!;
    public string CoverageName { get; set; } = null!;
    public string? ObjectTypeId { get; set; }
    public string? ObjectTypeCode { get; set; }
    public string? ObjectTypeName { get; set; }
    public string? ObjectTypeGroup { get; set; }
    public string? ObjectKind { get; set; }
    public List<DetailedAssessmentItemDto> Items { get; set; } = new();
}

public class DetailedAssessmentDocumentRowDto
{
    public string Id { get; set; } = null!;
    public string? Code { get; set; }
    public string? DocumentTypeId { get; set; }
    public string Name { get; set; } = null!;
    public bool? Complete { get; set; }
    public bool? IsCopy { get; set; }
    public string? Note { get; set; }
    public DateTime? IssueDate { get; set; }
    public List<DetailedAssessmentAttachmentDto> Attachments { get; set; } = new();
}

public class DetailedAssessmentDetailDto
{
    public Guid WorkTaskId { get; set; }
    public WorkTaskStatus WorkTaskStatus { get; set; }
    public Guid ClaimId { get; set; }
    public bool IsReporter { get; set; }
    public string? AssessorDeptName { get; set; }
    public string? AssessorName { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? DriverName { get; set; }
    public string? DriverSex { get; set; }
    public string? DriverPhone { get; set; }
    public string? DriverIdNo { get; set; }
    public string? DriverLicenseNo { get; set; }
    public DateTime? DriverLicenseEffectDate { get; set; }
    public DateTime? DriverLicenseExpireDate { get; set; }
    public string? DriverLicenseLevel { get; set; }
    public string? CarRegistryNo { get; set; }
    public DateTime? CarRegistryEffectDate { get; set; }
    public DateTime? CarRegistryExpireDate { get; set; }
    public List<DetailedAssessmentOptionDto> LicenseLevelOptions { get; set; } = new();
    public List<DetailedAssessmentOptionDto> CoverageOptions { get; set; } = new();
    public List<DetailedAssessmentOptionDto> RiskOptions { get; set; } = new();
    public List<DetailedAssessmentOptionDto> ClaimPlanOptions { get; set; } = new();
    public List<DetailedAssessmentOptionDto> ItemOptions { get; set; } = new();
    public List<DetailedAssessmentSectionDto> Sections { get; set; } = new();
    public List<DetailedAssessmentDocumentRowDto> DocumentRows { get; set; } = new();
}

public class SaveDetailedAssessmentInput
{
    public string? DriverName { get; set; }
    public string? DriverSex { get; set; }
    public string? DriverPhone { get; set; }
    public string? DriverIdNo { get; set; }
    public string? DriverLicenseNo { get; set; }
    public DateTime? DriverLicenseEffectDate { get; set; }
    public DateTime? DriverLicenseExpireDate { get; set; }
    public string? DriverLicenseLevel { get; set; }
    public string? CarRegistryNo { get; set; }
    public DateTime? CarRegistryEffectDate { get; set; }
    public DateTime? CarRegistryExpireDate { get; set; }
    public List<DetailedAssessmentSectionDto> Sections { get; set; } = new();
    public List<DetailedAssessmentDocumentRowDto> DocumentRows { get; set; } = new();
}
