using System;
using System.ComponentModel.DataAnnotations;
using iOne.ResMotorClasses;
using Volo.Abp.Application.Dtos;

namespace iOne.Master.ResMotorClasses;

public class ResMotorClassDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "ResMotorClass:Code")]
    public string Code { get; set; } = null!;

    [Display(Name = "ResMotorClass:Name")]
    public string Name { get; set; } = null!;

    [Display(Name = "ResMotorClass:Description")]
    public string? Description { get; set; }

    [Display(Name = "ResMotorClass:Status")]
    public ResMotorClassStatus Status { get; set; }
}

