using System.ComponentModel.DataAnnotations;
using iOne.ResReasonGroups;

namespace iOne.Master.ResReasons;

public class UpdateResReasonDto
{
    // ⚠️ QUAN TRỌNG: Không có Code property - Code không được phép sửa
    // ⚠️ QUAN TRỌNG: Không có GroupId property - GroupId không được phép sửa

    [Required(ErrorMessage = "ResReason:NameRequired")]
    [StringLength(250, ErrorMessage = "ResReason:NameMaxLength")]
    [Display(Name = "ResReason:Name")]
    public string Name { get; set; } = null!;

    [StringLength(500, ErrorMessage = "ResReason:DescriptionMaxLength")]
    [Display(Name = "ResReason:Description")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "ResReason:StatusRequired")]
    [Display(Name = "ResReason:Status")]
    public ResReasonGroupStatus Status { get; set; }
}
