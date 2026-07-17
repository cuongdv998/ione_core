using System.ComponentModel.DataAnnotations;
using iOne.ProCoverageTypes;

namespace iOne.Product.ProCoverageTypes;

public class CreateProCoverageTypeDto
{
    [Required(ErrorMessage = "ProCoverageType:CodeRequired")]
    [StringLength(50, ErrorMessage = "ProCoverageType:CodeMaxLength")]
    [RegularExpression(@"^[A-Z0-9_]+$", ErrorMessage = "ProCoverageType:CodeInvalidFormat")]
    [Display(Name = "ProCoverageType:Code")]
    public string Code { get; set; } = null!;

    [Required(ErrorMessage = "ProCoverageType:NameRequired")]
    [StringLength(250, ErrorMessage = "ProCoverageType:NameMaxLength")]
    [Display(Name = "ProCoverageType:Name")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "ProCoverageType:StatusRequired")]
    [Display(Name = "ProCoverageType:Status")]
    public ProCoverageTypeStatus Status { get; set; }

    [StringLength(500, ErrorMessage = "ProCoverageType:DescriptionMaxLength")]
    [Display(Name = "ProCoverageType:Description")]
    public string? Description { get; set; }
}

