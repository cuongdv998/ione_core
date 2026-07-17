using System.ComponentModel.DataAnnotations;
using iOne.ResMotorClasses;

namespace iOne.Master.ResMotorClasses;

public class CreateResMotorClassDto
{
    [Required(ErrorMessage = "ResMotorClass:CodeRequired")]
    [StringLength(50, ErrorMessage = "ResMotorClass:CodeMaxLength")]
    [RegularExpression(@"^[A-Z0-9_]+$", ErrorMessage = "ResMotorClass:CodeInvalid")]
    [Display(Name = "ResMotorClass:Code")]
    public string Code { get; set; } = null!;

    [Required(ErrorMessage = "ResMotorClass:NameRequired")]
    [StringLength(250, ErrorMessage = "ResMotorClass:NameMaxLength")]
    [Display(Name = "ResMotorClass:Name")]
    public string Name { get; set; } = null!;

    [StringLength(500, ErrorMessage = "ResMotorClass:DescriptionMaxLength")]
    [Display(Name = "ResMotorClass:Description")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "ResMotorClass:StatusRequired")]
    [Display(Name = "ResMotorClass:Status")]
    public ResMotorClassStatus Status { get; set; } = ResMotorClassStatus.Active;
}

