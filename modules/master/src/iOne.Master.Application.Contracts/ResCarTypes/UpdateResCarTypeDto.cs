using System.ComponentModel.DataAnnotations;
using iOne.ResCarTypes;

namespace iOne.Master.ResCarTypes;

public class UpdateResCarTypeDto
{
    // ⚠️ QUAN TRỌNG: Không có Code property - Code không được phép sửa

    [Required(ErrorMessage = "ResCarType:NameRequired")]
    [StringLength(250, ErrorMessage = "ResCarType:NameMaxLength")]
    [Display(Name = "ResCarType:Name")]
    public string Name { get; set; } = null!;

    [StringLength(500, ErrorMessage = "ResCarType:DescriptionMaxLength")]
    [Display(Name = "ResCarType:Description")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "ResCarType:StatusRequired")]
    [Display(Name = "ResCarType:Status")]
    public ResCarTypeStatus Status { get; set; }
}

