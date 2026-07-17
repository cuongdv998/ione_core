using System;
using System.ComponentModel.DataAnnotations;

namespace iOne.Workflow.WorkInstances;

public class InitWorkInstanceDto
{
    [Required]
    [StringLength(50)]
    public string WorkflowInstanceId { get; set; } = null!;

    [Required]
    [StringLength(50)]
    public string BusinessFlow { get; set; } = null!;

    [StringLength(36)]
    public string? BusinessCode { get; set; }

    [StringLength(50)]
    public string? BusinessName { get; set; }

    public Guid? BusinessKey { get; set; }
}
