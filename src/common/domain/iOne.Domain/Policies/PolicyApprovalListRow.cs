using System;
using iOne.WorkTasks;

namespace iOne.Policies;

/// <summary>
/// A policy with its work task business code and work task fields, used when querying the approval list with multiple or no business code filter.
/// </summary>
public class PolicyApprovalListRow
{
    public Policy Policy { get; set; } = null!;
    /// <summary>
    /// Policy version id (WorkTask.BusinessKey) – version đang chờ phê duyệt.
    /// </summary>
    public Guid PolicyVersionId { get; set; }
    public string BusinessCode { get; set; } = null!;
    public Guid WorkTaskId { get; set; }
    public WorkTaskStatus WorkTaskStatus { get; set; }
    public DateTime? WorkTaskCreationTime { get; set; }
}
