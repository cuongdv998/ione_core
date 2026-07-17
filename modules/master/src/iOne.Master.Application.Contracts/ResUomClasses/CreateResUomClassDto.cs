using System.ComponentModel.DataAnnotations;
using iOne.ResUomClasses;

namespace iOne.Master.ResUomClasses;

public class CreateResUomClassDto
{
    [Required(ErrorMessage = "ResUomClass:CodeRequired")]
    [StringLength(25, ErrorMessage = "ResUomClass:CodeMaxLength")]
    [RegularExpression(@"^[A-Z0-9_]+$", ErrorMessage = "ResUomClass:CodeInvalid")]
    [Display(Name = "ResUomClass:Code")]
    public string Code { get; set; } = null!;

    [Required(ErrorMessage = "ResUomClass:NameRequired")]
    [StringLength(100, ErrorMessage = "ResUomClass:NameMaxLength")]
    [Display(Name = "ResUomClass:Name")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "ResUomClass:StatusRequired")]
    [Display(Name = "ResUomClass:Status")]
    public ResUomClassStatus Status { get; set; } = ResUomClassStatus.Active;
}

