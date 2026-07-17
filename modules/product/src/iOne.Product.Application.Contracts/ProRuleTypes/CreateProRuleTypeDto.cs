using System.ComponentModel.DataAnnotations;
using iOne.ProRuleTypes;

namespace iOne.Product.ProRuleTypes;

public class CreateProRuleTypeDto
{
    [Required(ErrorMessage = "ProRuleType:CodeRequired")]
    [StringLength(50, ErrorMessage = "ProRuleType:CodeMaxLength")]
    [RegularExpression(@"^[A-Z0-9_]+$", ErrorMessage = "ProRuleType:CodeInvalid")]
    [Display(Name = "ProRuleType:Code")]
    public string Code { get; set; } = null!;

    [Required(ErrorMessage = "ProRuleType:NameRequired")]
    [StringLength(250, ErrorMessage = "ProRuleType:NameMaxLength")]
    [Display(Name = "ProRuleType:Name")]
    public string Name { get; set; } = null!;

    [StringLength(500, ErrorMessage = "ProRuleType:DescriptionMaxLength")]
    [Display(Name = "ProRuleType:Description")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "ProRuleType:StatusRequired")]
    [Display(Name = "ProRuleType:Status")]
    public ProRuleTypeStatus Status { get; set; } = ProRuleTypeStatus.Active;
}
