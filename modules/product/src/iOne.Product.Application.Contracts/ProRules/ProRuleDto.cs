using System;
using System.ComponentModel.DataAnnotations;
using iOne.ProRules;
using Volo.Abp.Application.Dtos;

namespace iOne.Product.ProRules;

public class ProRuleDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "ProRule:ApplyTo")]
    public string ApplyTo { get; set; } = null!;

    [Display(Name = "ProRule:ApplyToId")]
    public Guid ApplyToId { get; set; }

    [Display(Name = "ProRule:RuleTypeId")]
    public Guid RuleTypeId { get; set; }

    [Display(Name = "ProRule:Code")]
    public string Code { get; set; } = null!;

    [Display(Name = "ProRule:Name")]
    public string Name { get; set; } = null!;

    [Display(Name = "ProRule:Description")]
    public string? Description { get; set; }

    [Display(Name = "ProRule:RuleScript")]
    public string RuleScript { get; set; } = null!;

    [Display(Name = "ProRule:Priority")]
    public int Priority { get; set; } = 1;

    [Display(Name = "ProRule:Status")]
    public ProRuleStatus Status { get; set; }

    [Display(Name = "ProRule:EffectDate")]
    public DateTime EffectDate { get; set; }

    [Display(Name = "ProRule:ExpireDate")]
    public DateTime? ExpireDate { get; set; }

    [Display(Name = "ProRule:CreatorName")]
    public string? CreatorName { get; set; }
}
