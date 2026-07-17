using System.ComponentModel.DataAnnotations;
using iOne.ResMotorClasses;

namespace iOne.Master.ResMotorClasses;

public class UpdateResMotorClassDto
{
    // ⚠️ QUAN TRỌNG: Không có Code property - Code không được phép sửa

    [Required(ErrorMessage = "ResMotorClass:NameRequired")]
    [StringLength(250, ErrorMessage = "ResMotorClass:NameMaxLength")]
    [Display(Name = "ResMotorClass:Name")]
    public string Name { get; set; } = null!;

    [StringLength(500, ErrorMessage = "ResMotorClass:DescriptionMaxLength")]
    [Display(Name = "ResMotorClass:Description")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "ResMotorClass:StatusRequired")]
    [Display(Name = "ResMotorClass:Status")]
    public ResMotorClassStatus Status { get; set; }
}

