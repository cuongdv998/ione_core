using System;
using System.ComponentModel.DataAnnotations;
using iOne.ResBusinessAssignees;
using Volo.Abp.Application.Dtos;

namespace iOne.Master.ResBusinessAssignees;

public class ResBusinessAssigneeDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "ResBusinessAssignee:OrganizationId")]
    public Guid? OrganizationId { get; set; }

    [Display(Name = "ResBusinessAssignee:BusinessCode")]
    public string BusinessCode { get; set; } = null!;

    [Display(Name = "ResBusinessAssignee:AuthorityCode")]
    public string AuthorityCode { get; set; } = null!;

    [Display(Name = "ResBusinessAssignee:AssigneeType")]
    public ResBusinessAssigneeType AssigneeType { get; set; }

    [Display(Name = "ResBusinessAssignee:EffectDate")]
    public DateTime EffectDate { get; set; }

    [Display(Name = "ResBusinessAssignee:ExpireDate")]
    public DateTime? ExpireDate { get; set; }

    [Display(Name = "ResBusinessAssignee:AssigneeRole")]
    public string? AssigneeRole { get; set; }

    [Display(Name = "ResBusinessAssignee:AssigneeId")]
    public Guid? AssigneeId { get; set; }

    [Display(Name = "ResBusinessAssignee:DepartmentId")]
    public Guid? DepartmentId { get; set; }

    [Display(Name = "ResBusinessAssignee:DepartmentLevel")]
    public ResBusinessAssigneeDepartmentLevel? DepartmentLevel { get; set; }

    [Display(Name = "ResBusinessAssignee:Status")]
    public ResBusinessAssigneeStatus Status { get; set; }
}
