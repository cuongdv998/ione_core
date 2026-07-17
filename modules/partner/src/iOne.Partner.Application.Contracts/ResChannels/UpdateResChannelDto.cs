using System.ComponentModel.DataAnnotations;
using iOne.ResChannels;

namespace iOne.Partner.ResChannels;

public class UpdateResChannelDto
{
    // ⚠️ QUAN TRỌNG: Không có Code property - Code không được phép sửa

    [Required(ErrorMessage = "Partner::ResChannel:NameRequired")]
    [StringLength(50, ErrorMessage = "Partner::ResChannel:NameMaxLength")]
    [Display(Name = "Partner::ResChannel:Name")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Partner::ResChannel:StatusRequired")]
    [Display(Name = "Partner::ResChannel:Status")]
    public ResChannelStatus Status { get; set; }

    [StringLength(500, ErrorMessage = "Partner::ResChannel:DescriptionMaxLength")]
    [Display(Name = "Partner::ResChannel:Description")]
    public string? Description { get; set; }
}

