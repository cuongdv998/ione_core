using System;
using System.Collections.Generic;
using iOne.WorkTasks;

namespace iOne.Claim.Claims;

public class OnsiteAssessmentDetailDto
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

    public List<string> LossPositions { get; set; } = new();
    public bool HasLossThirdParty { get; set; }
    public string? WitnessTestimony { get; set; }
    public string? CauseDescription { get; set; }
    public string? Description { get; set; }
    public string? LocationDescription { get; set; }
    public string? DamageDescription { get; set; }
    public string? PartiesInvolvedDescription { get; set; }
    public string? AddressPlan { get; set; }
    public string? CustomerRecommendation { get; set; }
    public string? OtherDescription { get; set; }
    public Guid? GarageId { get; set; }
    public string? GarageName { get; set; }
    public DateTime? IssueDate { get; set; }

    public List<OnsiteAssessmentImageDto> Images { get; set; } = new();
    public List<OnsiteAssessmentDocumentDto> Documents { get; set; } = new();
}

public class OnsiteAssessmentImageDto
{
    public Guid ClaimDocumentId { get; set; }
    public Guid? DocumentId { get; set; }
    public Guid? DocumentTypeId { get; set; }
    public string? DocumentTypeName { get; set; }
    public string? FileName { get; set; }
    public string? Url { get; set; }
    public string? ThumbnailUrl { get; set; }
    public DateTime? UploadedAt { get; set; }
    public string? UploaderName { get; set; }
}

public class OnsiteAssessmentDocumentDto
{
    public Guid ClaimDocumentId { get; set; }
    public Guid? DocumentId { get; set; }
    public Guid? DocumentTypeId { get; set; }
    public string? DocumentTypeName { get; set; }
    public string? FileName { get; set; }
    public string? Url { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? Complete { get; set; }
    public string? IsCopy { get; set; }
    public string? Note { get; set; }
    public DateTime? IssueDate { get; set; }
}
