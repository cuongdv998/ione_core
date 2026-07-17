using System.ComponentModel.DataAnnotations;
using iOne.ResAppChannels;

namespace iOne.Master.ResAppChannels;

public class CreateResAppChannelDto
{
    [Required(ErrorMessage = "ResAppChannel:CodeRequired")]
    [StringLength(50, ErrorMessage = "ResAppChannel:CodeMaxLength")]
    [RegularExpression(@"^[A-Z0-9_]+$", ErrorMessage = "ResAppChannel:CodeInvalid")]
    [Display(Name = "ResAppChannel:Code")]
    public string Code { get; set; } = null!;

    [Required(ErrorMessage = "ResAppChannel:NameRequired")]
    [StringLength(50, ErrorMessage = "ResAppChannel:NameMaxLength")]
    [Display(Name = "ResAppChannel:Name")]
    public string Name { get; set; } = null!;

    [StringLength(500, ErrorMessage = "ResAppChannel:DescriptionMaxLength")]
    [Display(Name = "ResAppChannel:Description")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "ResAppChannel:StatusRequired")]
    [Display(Name = "ResAppChannel:Status")]
    public ResAppChannelStatus Status { get; set; } = ResAppChannelStatus.Active;

    [Display(Name = "ResAppChannel:Type")]
    public ResAppChannelType? Type { get; set; }
}
