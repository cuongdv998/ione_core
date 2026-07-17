using System;
using System.Collections.Generic;

namespace iOne.Claim.Claims;

public class DetailedAssessmentEvaluationItemDto
{
    public Guid EvaluateItemId { get; set; }

    public string Name { get; set; } = null!;

    public string? Result { get; set; }
}

public class DetailedAssessmentEvaluationDetailDto
{
    public Guid WorkTaskId { get; set; }

    public Guid? ClaimFolderId { get; set; }

    public string? Result { get; set; }

    public string? Description { get; set; }

    public List<DetailedAssessmentEvaluationItemDto> Items { get; set; } = new();
}

public class SaveDetailedAssessmentEvaluationItemInput
{
    public Guid EvaluateItemId { get; set; }

    public string Result { get; set; } = null!;
}

public class SaveDetailedAssessmentEvaluationInput
{
    public string Result { get; set; } = null!;

    public string? Description { get; set; }

    public List<SaveDetailedAssessmentEvaluationItemInput> Items { get; set; } = new();
}
