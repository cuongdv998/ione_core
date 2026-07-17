using System;
using System.ComponentModel.DataAnnotations;
using iOne.BusinessFlows;
using Volo.Abp.Application.Dtos;

namespace iOne.Master.BusinessFlows;

public class BusinessFlowDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "BusinessFlow:OrganizationId")]
    public Guid? OrganizationId { get; set; }

    [Display(Name = "BusinessFlow:InsurerId")]
    public Guid? InsurerId { get; set; }

    [Display(Name = "BusinessFlow:BusinessCode")]
    public string BusinessCode { get; set; } = null!;

    [Display(Name = "BusinessFlow:WorkflowName")]
    public string WorkflowName { get; set; } = null!;

    [Display(Name = "BusinessFlow:WorkflowVersion")]
    public string WorkflowVersion { get; set; } = null!;

    [Display(Name = "BusinessFlow:EffectDate")]
    public DateTime EffectDate { get; set; }

    [Display(Name = "BusinessFlow:ExpireDate")]
    public DateTime? ExpireDate { get; set; }

    [Display(Name = "BusinessFlow:Status")]
    public BusinessFlowStatus Status { get; set; }
}
