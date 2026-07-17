using System;
using System.ComponentModel.DataAnnotations;
using iOne.ResDamageLevels;
using Volo.Abp.Application.Dtos;

namespace iOne.Master.ResDamageLevels;

public class ResDamageLevelDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "ResDamageLevel:ObjectTypeId")]
    public Guid ObjectTypeId { get; set; }

    [Display(Name = "ResDamageLevel:Code")]
    public string Code { get; set; } = null!;

    [Display(Name = "ResDamageLevel:Name")]
    public string Name { get; set; } = null!;

    [Display(Name = "ResDamageLevel:Description")]
    public string? Description { get; set; }

    [Display(Name = "ResDamageLevel:Status")]
    public ResDamageLevelStatus Status { get; set; }
}

