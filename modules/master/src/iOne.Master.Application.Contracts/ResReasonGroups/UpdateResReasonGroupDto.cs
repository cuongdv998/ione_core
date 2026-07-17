using System.ComponentModel.DataAnnotations;
using iOne.ResReasonGroups;

namespace iOne.Master.ResReasonGroups;

public class UpdateResReasonGroupDto
{
    // ⚠️ QUAN TRỌNG: Không có Code property - Code không được phép sửa

    [Required(ErrorMessage = "ResReasonGroup:NameRequired")]
    [StringLength(250, ErrorMessage = "ResReasonGroup:NameMaxLength")]
    [Display(Name = "ResReasonGroup:Name")]
    public string Name { get; set; } = null!;

    [StringLength(500, ErrorMessage = "ResReasonGroup:DescriptionMaxLength")]
    [Display(Name = "ResReasonGroup:Description")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "ResReasonGroup:StatusRequired")]
    [Display(Name = "ResReasonGroup:Status")]
    public ResReasonGroupStatus Status { get; set; }
}
