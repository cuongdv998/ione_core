using System;

namespace iOne.Policy.Policies;

public class UpdatePrevEffectDateResultDto
{
    public bool Updated { get; set; }
    public Guid? PreviousPolicyVersionId { get; set; }
}

