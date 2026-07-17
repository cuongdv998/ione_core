namespace iOne.Policy.Policies;

/// <summary>
/// Optional input for submit-for-approval. ApproverId is the HrEmployee id; when empty or null, the system will determine the approver.
/// </summary>
public class SubmitForApprovalInput
{
    /// <summary>
    /// Optional approver (HrEmployee id). Send empty string or omit if system should auto-determine.
    /// </summary>
    public string? ApproverId { get; set; }
}
