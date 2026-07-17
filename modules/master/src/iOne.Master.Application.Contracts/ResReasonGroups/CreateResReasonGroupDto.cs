using System.ComponentModel.DataAnnotations;
using iOne.ResReasonGroups;

namespace iOne.Master.ResReasonGroups;

public class CreateResReasonGroupDto
{
    [Required(ErrorMessage = "ResReasonGroup:CodeRequired")]
    [StringLength(50, ErrorMessage = "ResReasonGroup:CodeMaxLength")]
    [RegularExpression(@"^[A-Z0-9_]+$", ErrorMessage = "ResReasonGroup:CodeInvalid")]
    [Display(Name = "ResReasonGroup:Code")]
    public string Code { get; set; } = null!;

    [Required(ErrorMessage = "ResReasonGroup:NameRequired")]
    [StringLength(250, ErrorMessage = "ResReasonGroup:NameMaxLength")]
    [Display(Name = "ResReasonGroup:Name")]
    public string Name { get; set; } = null!;

    [StringLength(500, ErrorMessage = "ResReasonGroup:DescriptionMaxLength")]
    [Display(Name = "ResReasonGroup:Description")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "ResReasonGroup:StatusRequired")]
    [Display(Name = "ResReasonGroup:Status")]
    public ResReasonGroupStatus Status { get; set; } = ResReasonGroupStatus.Active;
}
