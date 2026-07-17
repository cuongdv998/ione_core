using System;
using System.ComponentModel.DataAnnotations;
using iOne.ResCarTypes;
using Volo.Abp.Application.Dtos;

namespace iOne.Master.ResCarTypes;

public class ResCarTypeDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "ResCarType:Code")]
    public string Code { get; set; } = null!;

    [Display(Name = "ResCarType:Name")]
    public string Name { get; set; } = null!;

    [Display(Name = "ResCarType:Description")]
    public string? Description { get; set; }

    [Display(Name = "ResCarType:Status")]
    public ResCarTypeStatus Status { get; set; }
}

