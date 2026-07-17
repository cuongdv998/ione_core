using System;
using System.ComponentModel.DataAnnotations;
using iOne.HrEmployeePositions;
using Volo.Abp.Application.Dtos;

namespace iOne.Hr.HrEmployeePositions;

public class HrEmployeePositionDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "HrEmployeePosition:Code")]
    public string Code { get; set; } = null!;

    [Display(Name = "HrEmployeePosition:Name")]
    public string Name { get; set; } = null!;

    [Display(Name = "HrEmployeePosition:Type")]
    public HrEmployeePositionType Type { get; set; }

    [Display(Name = "HrEmployeePosition:Status")]
    public HrEmployeePositionStatus Status { get; set; }
}

