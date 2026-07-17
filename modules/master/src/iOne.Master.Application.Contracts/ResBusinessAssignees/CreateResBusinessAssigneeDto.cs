using System;
using System.ComponentModel.DataAnnotations;
using iOne.ResBusinessAssignees;

namespace iOne.Master.ResBusinessAssignees;

public class CreateResBusinessAssigneeDto
{
    [Display(Name = "ResBusinessAssignee:OrganizationId")]
    public Guid? OrganizationId { get; set; }

    [Required(ErrorMessage = "ResBusinessAssignee:BusinessCodeRequired")]
    [StringLength(50, ErrorMessage = "ResBusinessAssignee:BusinessCodeMaxLength")]
    [Display(Name = "ResBusinessAssignee:BusinessCode")]
    public string BusinessCode { get; set; } = null!;

    [Required(ErrorMessage = "ResBusinessAssignee:AuthorityCodeRequired")]
    [StringLength(50, ErrorMessage = "ResBusinessAssignee:AuthorityCodeMaxLength")]
    [Display(Name = "ResBusinessAssignee:AuthorityCode")]
    public string AuthorityCode { get; set; } = null!;

    [Required(ErrorMessage = "ResBusinessAssignee:AssigneeTypeRequired")]
    [Display(Name = "ResBusinessAssignee:AssigneeType")]
    public ResBusinessAssigneeType AssigneeType { get; set; }

    [Required(ErrorMessage = "ResBusinessAssignee:EffectDateRequired")]
    [Display(Name = "ResBusinessAssignee:EffectDate")]
    public DateTime EffectDate { get; set; }

    [Display(Name = "ResBusinessAssignee:ExpireDate")]
    public DateTime? ExpireDate { get; set; }

    [StringLength(50, ErrorMessage = "ResBusinessAssignee:AssigneeRoleMaxLength")]
    [Display(Name = "ResBusinessAssignee:AssigneeRole")]
    public string? AssigneeRole { get; set; }

    [Display(Name = "ResBusinessAssignee:AssigneeId")]
    public Guid? AssigneeId { get; set; }

    [Display(Name = "ResBusinessAssignee:DepartmentId")]
    public Guid? DepartmentId { get; set; }

    [Display(Name = "ResBusinessAssignee:DepartmentLevel")]
    public ResBusinessAssigneeDepartmentLevel? DepartmentLevel { get; set; }

    [Display(Name = "ResBusinessAssignee:Status")]
    public ResBusinessAssigneeStatus Status { get; set; } = ResBusinessAssigneeStatus.Active;
}
