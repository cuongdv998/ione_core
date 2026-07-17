using System.ComponentModel.DataAnnotations;
using iOne.ProRuleTypes;

namespace iOne.Product.ProRuleTypes;

public class UpdateProRuleTypeDto
{
    // ⚠️ QUAN TRỌNG: Không có Code property - Code không được phép sửa

    [Required(ErrorMessage = "ProRuleType:NameRequired")]
    [StringLength(250, ErrorMessage = "ProRuleType:NameMaxLength")]
    [Display(Name = "ProRuleType:Name")]
    public string Name { get; set; } = null!;

    [StringLength(500, ErrorMessage = "ProRuleType:DescriptionMaxLength")]
    [Display(Name = "ProRuleType:Description")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "ProRuleType:StatusRequired")]
    [Display(Name = "ProRuleType:Status")]
    public ProRuleTypeStatus Status { get; set; }
}
