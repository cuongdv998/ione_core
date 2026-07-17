using System;
using System.ComponentModel.DataAnnotations;
using iOne.ProRules;

namespace iOne.Product.ProRules;

public class UpdateProRuleDto
{
    // ⚠️ QUAN TRỌNG: Không có Code property - Code không được phép sửa

    [Required(ErrorMessage = "ProRule:ApplyToRequired")]
    [StringLength(15, ErrorMessage = "ProRule:ApplyToMaxLength")]
    [Display(Name = "ProRule:ApplyTo")]
    public string ApplyTo { get; set; } = null!;

    [Required(ErrorMessage = "ProRule:ApplyToIdRequired")]
    [Display(Name = "ProRule:ApplyToId")]
    public Guid ApplyToId { get; set; }

    [Required(ErrorMessage = "ProRule:RuleTypeIdRequired")]
    [Display(Name = "ProRule:RuleTypeId")]
    public Guid RuleTypeId { get; set; }

    [Required(ErrorMessage = "ProRule:NameRequired")]
    [StringLength(250, ErrorMessage = "ProRule:NameMaxLength")]
    [Display(Name = "ProRule:Name")]
    public string Name { get; set; } = null!;

    [StringLength(500, ErrorMessage = "ProRule:DescriptionMaxLength")]
    [Display(Name = "ProRule:Description")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "ProRule:RuleScriptRequired")]
    [Display(Name = "ProRule:RuleScript")]
    public string RuleScript { get; set; } = null!;

    [Display(Name = "ProRule:Priority")]
    [Range(1, 999, ErrorMessage = "ProRule:PriorityInvalid")]
    public int Priority { get; set; } = 1;

    [Required(ErrorMessage = "ProRule:StatusRequired")]
    [Display(Name = "ProRule:Status")]
    public ProRuleStatus Status { get; set; }

    [Required(ErrorMessage = "ProRule:EffectDateRequired")]
    [Display(Name = "ProRule:EffectDate")]
    public DateTime EffectDate { get; set; }

    [Display(Name = "ProRule:ExpireDate")]
    public DateTime? ExpireDate { get; set; }
}
