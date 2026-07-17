using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace iOne.Hr.HrEmployees;

public class HrEmployeeRoleRelDto : AuditedEntityDto<Guid>
{
    [Display(Name = "Hr::HrEmployeeRoleRel:EmployeeId")]
    public Guid EmployeeId { get; set; }

    [Display(Name = "Hr::HrEmployeeRoleRel:RoleId")]
    public Guid RoleId { get; set; }

    [Display(Name = "Hr::HrEmployeeRoleRel:RoleName")]
    public string? RoleName { get; set; }

    [Display(Name = "Hr::HrEmployeeRoleRel:EffectDate")]
    public DateTime EffectDate { get; set; }

    [Display(Name = "Hr::HrEmployeeRoleRel:ExpireDate")]
    public DateTime? ExpireDate { get; set; }
}

