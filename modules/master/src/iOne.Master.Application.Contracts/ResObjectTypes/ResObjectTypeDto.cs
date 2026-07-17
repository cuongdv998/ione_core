using System;
using System.ComponentModel.DataAnnotations;
using iOne.ResObjectTypes;
using Volo.Abp.Application.Dtos;

namespace iOne.Master.ResObjectTypes;

public class ResObjectTypeDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "ResObjectType:Code")]
    public string Code { get; set; } = null!;

    [Display(Name = "ResObjectType:Name")]
    public string Name { get; set; } = null!;

    [Display(Name = "ResObjectType:ObjectGroup")]
    public ResObjectGroup? ObjectGroup { get; set; }

    [Display(Name = "ResObjectType:Description")]
    public string? Description { get; set; }

    [Display(Name = "ResObjectType:Status")]
    public ResObjectTypeStatus Status { get; set; }
}

