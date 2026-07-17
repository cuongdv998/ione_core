using System;
using System.Collections.Generic;

namespace iOne.Claim.Claims;

public class SaveOnsiteAssessmentInput
{
    public string DriverName { get; set; } = string.Empty;
    public string DriverSex { get; set; } = string.Empty;
    public string DriverPhone { get; set; } = string.Empty;
    public string DriverIdNo { get; set; } = string.Empty;
    public string DriverLicenseNo { get; set; } = string.Empty;
    public DateTime? DriverLicenseEffectDate { get; set; }
    public DateTime? DriverLicenseExpireDate { get; set; }
    public string DriverLicenseLevel { get; set; } = string.Empty;
    public string CarRegistryNo { get; set; } = string.Empty;
    public DateTime? CarRegistryEffectDate { get; set; }
    public DateTime? CarRegistryExpireDate { get; set; }
    public List<string> LossPositions { get; set; } = new();
    public bool HasLossThirdParty { get; set; }
    public string WitnessTestimony { get; set; } = string.Empty;
    public string CauseDescription { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string LocationDescription { get; set; } = string.Empty;
    public string DamageDescription { get; set; } = string.Empty;
    public string PartiesInvolvedDescription { get; set; } = string.Empty;
    public string AddressPlan { get; set; } = string.Empty;
    public string CustomerRecommendation { get; set; } = string.Empty;
    public string? OtherDescription { get; set; }
    public Guid? GarageId { get; set; }
    public DateTime? IssueDate { get; set; }
    public List<SaveOnsiteImageInput> Images { get; set; } = new();
    public List<SaveOnsiteDocumentInput> Documents { get; set; } = new();
}

public class SaveOnsiteImageInput
{
    public Guid? ClaimDocumentId { get; set; }
    public Guid? DocumentId { get; set; }
    public Guid? DocumentTypeId { get; set; }
}

public class SaveOnsiteDocumentInput
{
    public Guid? ClaimDocumentId { get; set; }
    public Guid? DocumentId { get; set; }
    public Guid? DocumentTypeId { get; set; }
    public string? Note { get; set; }
    public string? Complete { get; set; }
    public string? IsCopy { get; set; }
    public DateTime? IssueDate { get; set; }
}
