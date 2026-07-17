using System;
using System.ComponentModel.DataAnnotations;
using iOne.HrEmployeeRoles;
using Volo.Abp.Application.Dtos;

namespace iOne.Hr.HrEmployeeRoles;

public class HrEmployeeRoleDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "HrEmployeeRole:Code")]
    public string Code { get; set; } = null!;

    [Display(Name = "HrEmployeeRole:Name")]
    public string Name { get; set; } = null!;

    [Display(Name = "HrEmployeeRole:Status")]
    public HrEmployeeRoleStatus Status { get; set; }
}

