using System.ComponentModel.DataAnnotations;
using iOne.PolicyTypes;

namespace iOne.Policy.PolicyTypes;

public class CreatePolicyTypeDto
{
    [Required(ErrorMessage = "PolicyType:CodeRequired")]
    [StringLength(50, ErrorMessage = "PolicyType:CodeMaxLength")]
    [RegularExpression(@"^[A-Z0-9_]+$", ErrorMessage = "PolicyType:CodeInvalid")]
    [Display(Name = "PolicyType:Code")]
    public string Code { get; set; } = null!;

    [Required(ErrorMessage = "PolicyType:NameRequired")]
    [StringLength(250, ErrorMessage = "PolicyType:NameMaxLength")]
    [Display(Name = "PolicyType:Name")]
    public string Name { get; set; } = null!;

    [StringLength(500, ErrorMessage = "PolicyType:DescriptionMaxLength")]
    [Display(Name = "PolicyType:Description")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "PolicyType:StatusRequired")]
    [Display(Name = "PolicyType:Status")]
    public PolicyTypeStatus Status { get; set; } = PolicyTypeStatus.Active;
}
