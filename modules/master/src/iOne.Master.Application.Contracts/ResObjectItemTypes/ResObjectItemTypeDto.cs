using System;
using System.ComponentModel.DataAnnotations;
using iOne.ResObjectItemTypes;
using Volo.Abp.Application.Dtos;

namespace iOne.Master.ResObjectItemTypes;

public class ResObjectItemTypeDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "ResObjectItemType:Code")]
    public string Code { get; set; } = null!;

    [Display(Name = "ResObjectItemType:Name")]
    public string Name { get; set; } = null!;

    [Display(Name = "ResObjectItemType:Description")]
    public string? Description { get; set; }

    [Display(Name = "ResObjectItemType:Status")]
    public ResObjectItemTypeStatus Status { get; set; }
}
