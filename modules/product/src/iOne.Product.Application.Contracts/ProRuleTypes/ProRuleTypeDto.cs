using System;
using System.ComponentModel.DataAnnotations;
using iOne.ProRuleTypes;
using Volo.Abp.Application.Dtos;

namespace iOne.Product.ProRuleTypes;

public class ProRuleTypeDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "ProRuleType:Code")]
    public string Code { get; set; } = null!;

    [Display(Name = "ProRuleType:Name")]
    public string Name { get; set; } = null!;

    [Display(Name = "ProRuleType:Description")]
    public string? Description { get; set; }

    [Display(Name = "ProRuleType:Status")]
    public ProRuleTypeStatus Status { get; set; }
}
