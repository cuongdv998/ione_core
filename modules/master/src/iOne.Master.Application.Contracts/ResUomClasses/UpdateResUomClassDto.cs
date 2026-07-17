using System.ComponentModel.DataAnnotations;
using iOne.ResUomClasses;

namespace iOne.Master.ResUomClasses;

public class UpdateResUomClassDto
{
    // ⚠️ QUAN TRỌNG: Không có Code property - Code không được phép sửa

    [Required(ErrorMessage = "ResUomClass:NameRequired")]
    [StringLength(100, ErrorMessage = "ResUomClass:NameMaxLength")]
    [Display(Name = "ResUomClass:Name")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "ResUomClass:StatusRequired")]
    [Display(Name = "ResUomClass:Status")]
    public ResUomClassStatus Status { get; set; }
}

