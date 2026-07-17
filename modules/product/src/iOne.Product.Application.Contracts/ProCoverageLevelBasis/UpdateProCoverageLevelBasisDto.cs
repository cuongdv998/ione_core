using System.ComponentModel.DataAnnotations;
using iOne.ProCoverageLevelBases;

namespace iOne.Product.ProCoverageLevelBasis;

public class UpdateProCoverageLevelBasisDto
{
    // ⚠️ QUAN TRỌNG: Không có Code property - Code không được phép sửa

    [Required(ErrorMessage = "ProCoverageLevelBasis:NameRequired")]
    [StringLength(250, ErrorMessage = "ProCoverageLevelBasis:NameMaxLength")]
    [Display(Name = "ProCoverageLevelBasis:Name")]
    public string Name { get; set; } = null!;

    [StringLength(500, ErrorMessage = "ProCoverageLevelBasis:DescriptionMaxLength")]
    [Display(Name = "ProCoverageLevelBasis:Description")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "ProCoverageLevelBasis:StatusRequired")]
    [Display(Name = "ProCoverageLevelBasis:Status")]
    public ProCoverageLevelBasisStatus Status { get; set; }
}
