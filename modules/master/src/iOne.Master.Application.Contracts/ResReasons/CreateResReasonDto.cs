using System;
using System.ComponentModel.DataAnnotations;
using iOne.ResReasonGroups;

namespace iOne.Master.ResReasons;

public class CreateResReasonDto
{
    [Required(ErrorMessage = "ResReason:GroupIdRequired")]
    [Display(Name = "ResReason:GroupId")]
    public Guid GroupId { get; set; }

    [Required(ErrorMessage = "ResReason:CodeRequired")]
    [StringLength(50, ErrorMessage = "ResReason:CodeMaxLength")]
    [RegularExpression(@"^[A-Z0-9_]+$", ErrorMessage = "ResReason:CodeInvalid")]
    [Display(Name = "ResReason:Code")]
    public string Code { get; set; } = null!;

    [Required(ErrorMessage = "ResReason:NameRequired")]
    [StringLength(250, ErrorMessage = "ResReason:NameMaxLength")]
    [Display(Name = "ResReason:Name")]
    public string Name { get; set; } = null!;

    [StringLength(500, ErrorMessage = "ResReason:DescriptionMaxLength")]
    [Display(Name = "ResReason:Description")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "ResReason:StatusRequired")]
    [Display(Name = "ResReason:Status")]
    public ResReasonGroupStatus Status { get; set; } = ResReasonGroupStatus.Active;
}
