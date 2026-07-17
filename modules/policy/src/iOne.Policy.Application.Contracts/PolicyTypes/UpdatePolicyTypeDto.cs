using System.ComponentModel.DataAnnotations;
using iOne.PolicyTypes;

namespace iOne.Policy.PolicyTypes;

public class UpdatePolicyTypeDto
{
    // ⚠️ QUAN TRỌNG: Không có Code property - Code không được phép sửa

    [Required(ErrorMessage = "PolicyType:NameRequired")]
    [StringLength(250, ErrorMessage = "PolicyType:NameMaxLength")]
    [Display(Name = "PolicyType:Name")]
    public string Name { get; set; } = null!;

    [StringLength(500, ErrorMessage = "PolicyType:DescriptionMaxLength")]
    [Display(Name = "PolicyType:Description")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "PolicyType:StatusRequired")]
    [Display(Name = "PolicyType:Status")]
    public PolicyTypeStatus Status { get; set; }
}
