namespace iOne.Policy.Policies;

public class UpdatePolicyStatusInput
{
    public string PolicyVersionId { get; set; } = null!;

    /// <summary>
    /// "approve" or "reject"
    /// </summary>
    public string Action { get; set; } = null!;
}
