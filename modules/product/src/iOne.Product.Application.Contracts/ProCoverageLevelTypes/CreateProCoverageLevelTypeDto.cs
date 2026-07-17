using System.ComponentModel.DataAnnotations;
using iOne.ProCoverageLevelTypes;

namespace iOne.Product.ProCoverageLevelTypes;

public class CreateProCoverageLevelTypeDto
{
    [Required(ErrorMessage = "ProCoverageLevelType:CodeRequired")]
    [StringLength(50, ErrorMessage = "ProCoverageLevelType:CodeMaxLength")]
    [RegularExpression(@"^[A-Z0-9_]+$", ErrorMessage = "ProCoverageLevelType:CodeInvalid")]
    [Display(Name = "ProCoverageLevelType:Code")]
    public string Code { get; set; } = null!;

    [Required(ErrorMessage = "ProCoverageLevelType:NameRequired")]
    [StringLength(250, ErrorMessage = "ProCoverageLevelType:NameMaxLength")]
    [Display(Name = "ProCoverageLevelType:Name")]
    public string Name { get; set; } = null!;

    [StringLength(500, ErrorMessage = "ProCoverageLevelType:DescriptionMaxLength")]
    [Display(Name = "ProCoverageLevelType:Description")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "ProCoverageLevelType:StatusRequired")]
    [Display(Name = "ProCoverageLevelType:Status")]
    public ProCoverageLevelTypeStatus Status { get; set; } = ProCoverageLevelTypeStatus.Active;
}
