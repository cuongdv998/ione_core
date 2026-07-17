using System.ComponentModel.DataAnnotations;
using iOne.ResChannels;

namespace iOne.Partner.ResChannels;

public class CreateResChannelDto
{
    [Required(ErrorMessage = "Partner::ResChannel:CodeRequired")]
    [StringLength(50, ErrorMessage = "Partner::ResChannel:CodeMaxLength")]
    [RegularExpression(@"^[A-Z0-9_]+$", ErrorMessage = "Partner::ResChannel:CodeInvalid")]
    [Display(Name = "Partner::ResChannel:Code")]
    public string Code { get; set; } = null!;

    [Required(ErrorMessage = "Partner::ResChannel:NameRequired")]
    [StringLength(50, ErrorMessage = "Partner::ResChannel:NameMaxLength")]
    [Display(Name = "Partner::ResChannel:Name")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Partner::ResChannel:StatusRequired")]
    [Display(Name = "Partner::ResChannel:Status")]
    public ResChannelStatus Status { get; set; } = ResChannelStatus.Active;

    [StringLength(500, ErrorMessage = "Partner::ResChannel:DescriptionMaxLength")]
    [Display(Name = "Partner::ResChannel:Description")]
    public string? Description { get; set; }
}

