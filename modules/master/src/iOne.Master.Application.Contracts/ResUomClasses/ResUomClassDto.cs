using System;
using System.ComponentModel.DataAnnotations;
using iOne.ResUomClasses;
using Volo.Abp.Application.Dtos;

namespace iOne.Master.ResUomClasses;

public class ResUomClassDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "ResUomClass:Code")]
    public string Code { get; set; } = null!;

    [Display(Name = "ResUomClass:Name")]
    public string Name { get; set; } = null!;

    [Display(Name = "ResUomClass:Status")]
    public ResUomClassStatus Status { get; set; }
}

