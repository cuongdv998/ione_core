using System.Collections.Generic;

namespace iOne.Policy.Policies;

public class PartnerCreatePolicyResultDto
{
    public bool Success { get; set; }

    public List<string> ErrorMessages { get; set; } = new();
}
