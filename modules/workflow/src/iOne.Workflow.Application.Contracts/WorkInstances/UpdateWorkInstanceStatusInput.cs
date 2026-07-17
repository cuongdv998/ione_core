namespace iOne.Workflow.WorkInstances;

public class UpdateWorkInstanceStatusInput
{
    /// <summary>
    /// Workflow instance id (string) used to look up the work instance.
    /// </summary>
    public string WorkInstanceId { get; set; } = null!;

    /// <summary>
    /// Status name, e.g. "New", "InProgress", "Completed", "Rejected", "Approved", "Cancelled", "WaitApprove", "Pending", "Return", "Accepted".
    /// </summary>
    public string Status { get; set; } = null!;
}
