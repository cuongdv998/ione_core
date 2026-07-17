using System;
using System.ComponentModel.DataAnnotations;

namespace iOne.Hr.HrEmployees;

public class UpdateHrEmployeeRoleRelDto
{
    [Required]
    [Display(Name = "Hr::HrEmployeeRoleRel:RoleId")]
    public Guid RoleId { get; set; }

    [Required]
    [Display(Name = "Hr::HrEmployeeRoleRel:EffectDate")]
    public DateTime EffectDate { get; set; }

    [Display(Name = "Hr::HrEmployeeRoleRel:ExpireDate")]
    public DateTime? ExpireDate { get; set; }
}

