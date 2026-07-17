using System;
using System.ComponentModel.DataAnnotations;
using iOne.HrDepartmentTypes;
using Volo.Abp.Application.Dtos;

namespace iOne.Hr.HrDepartmentTypes;

public class HrDepartmentTypeDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "HrDepartmentType:Code")]
    public string Code { get; set; } = null!;

    [Display(Name = "HrDepartmentType:Name")]
    public string Name { get; set; } = null!;

    [Display(Name = "HrDepartmentType:Status")]
    public HrDepartmentTypeStatus Status { get; set; }
}

