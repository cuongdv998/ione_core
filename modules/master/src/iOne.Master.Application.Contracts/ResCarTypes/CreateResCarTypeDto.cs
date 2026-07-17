using System.ComponentModel.DataAnnotations;
using iOne.ResCarTypes;

namespace iOne.Master.ResCarTypes;

public class CreateResCarTypeDto
{
    [Required(ErrorMessage = "ResCarType:CodeRequired")]
    [StringLength(50, ErrorMessage = "ResCarType:CodeMaxLength")]
    [RegularExpression(@"^[A-Z0-9_]+$", ErrorMessage = "ResCarType:CodeInvalid")]
    [Display(Name = "ResCarType:Code")]
    public string Code { get; set; } = null!;

    [Required(ErrorMessage = "ResCarType:NameRequired")]
    [StringLength(250, ErrorMessage = "ResCarType:NameMaxLength")]
    [Display(Name = "ResCarType:Name")]
    public string Name { get; set; } = null!;

    [StringLength(500, ErrorMessage = "ResCarType:DescriptionMaxLength")]
    [Display(Name = "ResCarType:Description")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "ResCarType:StatusRequired")]
    [Display(Name = "ResCarType:Status")]
    public ResCarTypeStatus Status { get; set; } = ResCarTypeStatus.Active;
}

