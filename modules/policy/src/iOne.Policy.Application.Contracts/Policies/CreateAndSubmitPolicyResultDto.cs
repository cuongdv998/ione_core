using System;

namespace iOne.Policy.Policies;

public class CreateAndSubmitPolicyResultDto
{
    public Guid? PolicyId { get; set; }

    public string? PolicyNo { get; set; }

    public bool Created { get; set; }

    public bool Submitted { get; set; }

    public string? SubmitErrorCode { get; set; }

    public string? SubmitErrorMessage { get; set; }
}
