using System.ComponentModel.DataAnnotations;
using iOne.ProCoverageLevelBases;

namespace iOne.Product.ProCoverageLevelBasis;

public class CreateProCoverageLevelBasisDto
{
    [Required(ErrorMessage = "ProCoverageLevelBasis:CodeRequired")]
    [StringLength(50, ErrorMessage = "ProCoverageLevelBasis:CodeMaxLength")]
    [RegularExpression(@"^[A-Z0-9_]+$", ErrorMessage = "ProCoverageLevelBasis:CodeInvalid")]
    [Display(Name = "ProCoverageLevelBasis:Code")]
    public string Code { get; set; } = null!;

    [Required(ErrorMessage = "ProCoverageLevelBasis:NameRequired")]
    [StringLength(250, ErrorMessage = "ProCoverageLevelBasis:NameMaxLength")]
    [Display(Name = "ProCoverageLevelBasis:Name")]
    public string Name { get; set; } = null!;

    [StringLength(500, ErrorMessage = "ProCoverageLevelBasis:DescriptionMaxLength")]
    [Display(Name = "ProCoverageLevelBasis:Description")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "ProCoverageLevelBasis:StatusRequired")]
    [Display(Name = "ProCoverageLevelBasis:Status")]
    public ProCoverageLevelBasisStatus Status { get; set; } = ProCoverageLevelBasisStatus.Active;
}
