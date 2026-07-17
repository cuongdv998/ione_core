using System.ComponentModel.DataAnnotations;
using iOne.ResObjectTypes;

namespace iOne.Master.ResObjectTypes;

public class CreateResObjectTypeDto
{
    [Required(ErrorMessage = "ResObjectType:CodeRequired")]
    [StringLength(50, ErrorMessage = "ResObjectType:CodeMaxLength")]
    [RegularExpression(@"^[A-Z0-9_]+$", ErrorMessage = "ResObjectType:CodeInvalid")]
    [Display(Name = "ResObjectType:Code")]
    public string Code { get; set; } = null!;

    [Required(ErrorMessage = "ResObjectType:NameRequired")]
    [StringLength(250, ErrorMessage = "ResObjectType:NameMaxLength")]
    [Display(Name = "ResObjectType:Name")]
    public string Name { get; set; } = null!;

    [Display(Name = "ResObjectType:ObjectGroup")]
    public ResObjectGroup? ObjectGroup { get; set; }

    [StringLength(500, ErrorMessage = "ResObjectType:DescriptionMaxLength")]
    [Display(Name = "ResObjectType:Description")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "ResObjectType:StatusRequired")]
    [Display(Name = "ResObjectType:Status")]
    public ResObjectTypeStatus Status { get; set; } = ResObjectTypeStatus.Active;
}

