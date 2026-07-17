using System.ComponentModel.DataAnnotations;
using iOne.ProCoverageGroups;

namespace iOne.Product.ProCoverageGroups;

public class UpdateProCoverageGroupDto
{
    // ⚠️ QUAN TRỌNG: Không có Code - Code không được phép sửa

    [Required(ErrorMessage = "ProCoverageGroup:NameRequired")]
    [StringLength(250, ErrorMessage = "ProCoverageGroup:NameMaxLength")]
    [Display(Name = "ProCoverageGroup:Name")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "ProCoverageGroup:StatusRequired")]
    [Display(Name = "ProCoverageGroup:Status")]
    public ProCoverageGroupStatus Status { get; set; }

    [StringLength(500, ErrorMessage = "ProCoverageGroup:DescriptionMaxLength")]
    [Display(Name = "ProCoverageGroup:Description")]
    public string? Description { get; set; }
}

