using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace iOne.Policy.PolicyRequestApproval;

/// <summary>
/// Input for approving multiple policy request approvals in one batch.
/// </summary>
public class ApproveBatchRequest
{
    /// <summary>
    /// Work task IDs to approve. All must be assigned to the current user and in WaitApprove status.
    /// </summary>
    [Required]
    [MinLength(1)]
    public List<Guid> WorkTaskIds { get; set; } = new();
}
