using System.ComponentModel.DataAnnotations;
using iOne.ResObjectItemTypes;

namespace iOne.Master.ResObjectItemTypes;

public class CreateResObjectItemTypeDto
{
    [Required(ErrorMessage = "ResObjectItemType:CodeRequired")]
    [StringLength(50, ErrorMessage = "ResObjectItemType:CodeMaxLength")]
    [RegularExpression(@"^[A-Z0-9_]+$", ErrorMessage = "ResObjectItemType:CodeInvalid")]
    [Display(Name = "ResObjectItemType:Code")]
    public string Code { get; set; } = null!;

    [Required(ErrorMessage = "ResObjectItemType:NameRequired")]
    [StringLength(250, ErrorMessage = "ResObjectItemType:NameMaxLength")]
    [Display(Name = "ResObjectItemType:Name")]
    public string Name { get; set; } = null!;

    [StringLength(500, ErrorMessage = "ResObjectItemType:DescriptionMaxLength")]
    [Display(Name = "ResObjectItemType:Description")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "ResObjectItemType:StatusRequired")]
    [Display(Name = "ResObjectItemType:Status")]
    public ResObjectItemTypeStatus Status { get; set; } = ResObjectItemTypeStatus.Active;
}
