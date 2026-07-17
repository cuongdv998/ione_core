using System.ComponentModel.DataAnnotations;
using iOne.ResCarLines;

namespace iOne.Master.ResCarLines;

public class UpdateResCarLineDto
{
    // ⚠️ QUAN TRỌNG: Không có Code property - Code không được phép sửa

    [Required(ErrorMessage = "ResCarLine:NameRequired")]
    [StringLength(250, ErrorMessage = "ResCarLine:NameMaxLength")]
    [Display(Name = "ResCarLine:Name")]
    public string Name { get; set; } = null!;

    [StringLength(500, ErrorMessage = "ResCarLine:DescriptionMaxLength")]
    [Display(Name = "ResCarLine:Description")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "ResCarLine:StatusRequired")]
    [Display(Name = "ResCarLine:Status")]
    public ResCarLineStatus Status { get; set; }
}


