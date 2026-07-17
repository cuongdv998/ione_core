using System;
using System.ComponentModel.DataAnnotations;
using iOne.ResEvents;
using Volo.Abp.Application.Dtos;

namespace iOne.Master.ResEvents;

public class ResEventDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "ResEvent:Code")]
    public string Code { get; set; } = null!;

    [Display(Name = "ResEvent:Name")]
    public string Name { get; set; } = null!;

    [Display(Name = "ResEvent:Description")]
    public string? Description { get; set; }

    [Display(Name = "ResEvent:Status")]
    public ResEventStatus Status { get; set; }
}

