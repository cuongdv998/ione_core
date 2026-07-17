using System;
using System.ComponentModel.DataAnnotations;
using iOne.BusinessFlows;

namespace iOne.Master.BusinessFlows;

public class CreateBusinessFlowDto
{
    [Display(Name = "BusinessFlow:OrganizationId")]
    public Guid? OrganizationId { get; set; }

    [Display(Name = "BusinessFlow:InsurerId")]
    public Guid? InsurerId { get; set; }

    [Required(ErrorMessage = "BusinessFlow:BusinessCodeRequired")]
    [StringLength(50, ErrorMessage = "BusinessFlow:BusinessCodeMaxLength")]
    [Display(Name = "BusinessFlow:BusinessCode")]
    public string BusinessCode { get; set; } = null!;

    [Required(ErrorMessage = "BusinessFlow:WorkflowNameRequired")]
    [StringLength(50, ErrorMessage = "BusinessFlow:WorkflowNameMaxLength")]
    [Display(Name = "BusinessFlow:WorkflowName")]
    public string WorkflowName { get; set; } = null!;

    [Required(ErrorMessage = "BusinessFlow:WorkflowVersionRequired")]
    [StringLength(50, ErrorMessage = "BusinessFlow:WorkflowVersionMaxLength")]
    [Display(Name = "BusinessFlow:WorkflowVersion")]
    public string WorkflowVersion { get; set; } = null!;

    [Required(ErrorMessage = "BusinessFlow:EffectDateRequired")]
    [Display(Name = "BusinessFlow:EffectDate")]
    public DateTime EffectDate { get; set; }

    [Display(Name = "BusinessFlow:ExpireDate")]
    public DateTime? ExpireDate { get; set; }
}
