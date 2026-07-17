using System.ComponentModel.DataAnnotations;
using iOne.ResAppChannels;

namespace iOne.Master.ResAppChannels;

public class UpdateResAppChannelDto
{
    // ⚠️ QUAN TRỌNG: Không có Code property - Code không được phép sửa

    [Required(ErrorMessage = "ResAppChannel:NameRequired")]
    [StringLength(50, ErrorMessage = "ResAppChannel:NameMaxLength")]
    [Display(Name = "ResAppChannel:Name")]
    public string Name { get; set; } = null!;

    [StringLength(500, ErrorMessage = "ResAppChannel:DescriptionMaxLength")]
    [Display(Name = "ResAppChannel:Description")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "ResAppChannel:StatusRequired")]
    [Display(Name = "ResAppChannel:Status")]
    public ResAppChannelStatus Status { get; set; }

    [Display(Name = "ResAppChannel:Type")]
    public ResAppChannelType? Type { get; set; }
}
