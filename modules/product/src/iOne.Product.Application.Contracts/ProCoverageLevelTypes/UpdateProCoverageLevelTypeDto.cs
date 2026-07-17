using System.ComponentModel.DataAnnotations;
using iOne.ProCoverageLevelTypes;

namespace iOne.Product.ProCoverageLevelTypes;

public class UpdateProCoverageLevelTypeDto
{
    // ⚠️ QUAN TRỌNG: Không có Code property - Code không được phép sửa

    [Required(ErrorMessage = "ProCoverageLevelType:NameRequired")]
    [StringLength(250, ErrorMessage = "ProCoverageLevelType:NameMaxLength")]
    [Display(Name = "ProCoverageLevelType:Name")]
    public string Name { get; set; } = null!;

    [StringLength(500, ErrorMessage = "ProCoverageLevelType:DescriptionMaxLength")]
    [Display(Name = "ProCoverageLevelType:Description")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "ProCoverageLevelType:StatusRequired")]
    [Display(Name = "ProCoverageLevelType:Status")]
    public ProCoverageLevelTypeStatus Status { get; set; }
}
