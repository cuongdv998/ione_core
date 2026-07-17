using System;
using System.ComponentModel.DataAnnotations;

namespace iOne.Workflow.WorkTasks;

public class InitWorkTaskDto
{
    [Required]
    [StringLength(50)]
    public string WorkflowInstanceId { get; set; } = null!;

    [StringLength(50)]
    public string? BusinessFlow { get; set; }

    [Required]
    [StringLength(50)]
    public string BusinessCode { get; set; } = null!;

    [Required]
    [StringLength(50)]
    public string BusinessName { get; set; } = null!;

    [Required]
    public Guid BusinessKey { get; set; }

    [StringLength(50)]
    public string? BusinessAuthorityCode { get; set; }

    [Required]
    [StringLength(250)]
    public string Name { get; set; } = null!;

    [Required]
    [StringLength(15)]
    public string Status { get; set; } = null!;

    [Required]
    [StringLength(15)]
    public string Priority { get; set; } = null!;

    [Required]
    [StringLength(50)]
    public string TaskCategory { get; set; } = null!;

    [StringLength(20)]
    public string? Sla { get; set; }

    /// <summary>
    /// Optional assignee override. If not blank, work_task.AssigneeId is set to this (parsed as Guid); otherwise assignee is resolved from ResBusinessAssignee.
    /// </summary>
    [StringLength(36)]
    public string? AssigneeId { get; set; }

    public string? ReporterId { get; set; }
}
