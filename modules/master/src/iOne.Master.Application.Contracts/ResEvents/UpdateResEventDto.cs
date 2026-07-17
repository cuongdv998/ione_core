using System.ComponentModel.DataAnnotations;
using iOne.ResEvents;

namespace iOne.Master.ResEvents;

public class UpdateResEventDto
{
    // ⚠️ QUAN TRỌNG: Không có Code property - Code không được phép sửa

    [Required(ErrorMessage = "ResEvent:NameRequired")]
    [StringLength(50, ErrorMessage = "ResEvent:NameMaxLength")]
    [Display(Name = "ResEvent:Name")]
    public string Name { get; set; } = null!;

    [StringLength(500, ErrorMessage = "ResEvent:DescriptionMaxLength")]
    [Display(Name = "ResEvent:Description")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "ResEvent:StatusRequired")]
    [Display(Name = "ResEvent:Status")]
    public ResEventStatus Status { get; set; }
}

