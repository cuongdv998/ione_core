using System;
using System.ComponentModel.DataAnnotations;

namespace iOne.Master.BusinessFlows;

public class UpdateBusinessFlowDto
{
    // Chỉ cho phép sửa WorkflowName, WorkflowVersion, ExpireDate

    [Required(ErrorMessage = "BusinessFlow:WorkflowNameRequired")]
    [StringLength(50, ErrorMessage = "BusinessFlow:WorkflowNameMaxLength")]
    [Display(Name = "BusinessFlow:WorkflowName")]
    public string WorkflowName { get; set; } = null!;

    [Required(ErrorMessage = "BusinessFlow:WorkflowVersionRequired")]
    [StringLength(50, ErrorMessage = "BusinessFlow:WorkflowVersionMaxLength")]
    [Display(Name = "BusinessFlow:WorkflowVersion")]
    public string WorkflowVersion { get; set; } = null!;

    [Display(Name = "BusinessFlow:ExpireDate")]
    public DateTime? ExpireDate { get; set; }
}
