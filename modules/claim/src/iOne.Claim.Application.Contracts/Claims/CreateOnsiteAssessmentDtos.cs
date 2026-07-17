using System;
using System.Collections.Generic;

namespace iOne.Claim.Claims;

public class CreateOnsiteAssessmentRequestInput
{
    public Guid ClaimId { get; set; }
    public Guid? AssigneeOrganizationId { get; set; }
    public Guid? AssigneeId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class OnsiteAssessmentCreateRequestDto
{
    public Guid ClaimId { get; set; }
    public string ClaimCode { get; set; } = string.Empty;
    public DateTime? NotifyDate { get; set; }
    public string? NotifierName { get; set; }
    public string? CarPlate { get; set; }
}

public class OnsiteAssessmentCreateResultDto
{
    public Guid ClaimId { get; set; }
    public Guid WorkTaskId { get; set; }
}
