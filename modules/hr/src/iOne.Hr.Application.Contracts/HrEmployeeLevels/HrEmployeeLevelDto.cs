using System;
using System.ComponentModel.DataAnnotations;
using iOne.HrEmployeeLevels;
using Volo.Abp.Application.Dtos;

namespace iOne.Hr.HrEmployeeLevels;

public class HrEmployeeLevelDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "HrEmployeeLevel:Code")]
    public string Code { get; set; } = null!;

    [Display(Name = "HrEmployeeLevel:Name")]
    public string Name { get; set; } = null!;

    [Display(Name = "HrEmployeeLevel:Status")]
    public HrEmployeeLevelStatus Status { get; set; }

    [Display(Name = "HrEmployeeLevel:Description")]
    public string? Description { get; set; }
}

