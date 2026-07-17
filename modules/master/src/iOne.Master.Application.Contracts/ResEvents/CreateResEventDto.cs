using System.ComponentModel.DataAnnotations;
using iOne.ResEvents;

namespace iOne.Master.ResEvents;

public class CreateResEventDto
{
    [Required(ErrorMessage = "ResEvent:CodeRequired")]
    [StringLength(50, ErrorMessage = "ResEvent:CodeMaxLength")]
    [RegularExpression(@"^[A-Z0-9_]+$", ErrorMessage = "ResEvent:CodeInvalid")]
    [Display(Name = "ResEvent:Code")]
    public string Code { get; set; } = null!;

    [Required(ErrorMessage = "ResEvent:NameRequired")]
    [StringLength(50, ErrorMessage = "ResEvent:NameMaxLength")]
    [Display(Name = "ResEvent:Name")]
    public string Name { get; set; } = null!;

    [StringLength(500, ErrorMessage = "ResEvent:DescriptionMaxLength")]
    [Display(Name = "ResEvent:Description")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "ResEvent:StatusRequired")]
    [Display(Name = "ResEvent:Status")]
    public ResEventStatus Status { get; set; } = ResEventStatus.Active;
}

