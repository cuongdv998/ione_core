using System.ComponentModel.DataAnnotations;
using iOne.ResCarLines;

namespace iOne.Master.ResCarLines;

public class CreateResCarLineDto
{
    [Required(ErrorMessage = "ResCarLine:CodeRequired")]
    [StringLength(50, ErrorMessage = "ResCarLine:CodeMaxLength")]
    [RegularExpression(@"^[A-Z0-9_]+$", ErrorMessage = "ResCarLine:CodeInvalid")]
    [Display(Name = "ResCarLine:Code")]
    public string Code { get; set; } = null!;

    [Required(ErrorMessage = "ResCarLine:NameRequired")]
    [StringLength(250, ErrorMessage = "ResCarLine:NameMaxLength")]
    [Display(Name = "ResCarLine:Name")]
    public string Name { get; set; } = null!;

    [StringLength(500, ErrorMessage = "ResCarLine:DescriptionMaxLength")]
    [Display(Name = "ResCarLine:Description")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "ResCarLine:StatusRequired")]
    [Display(Name = "ResCarLine:Status")]
    public ResCarLineStatus Status { get; set; } = ResCarLineStatus.Active;
}


