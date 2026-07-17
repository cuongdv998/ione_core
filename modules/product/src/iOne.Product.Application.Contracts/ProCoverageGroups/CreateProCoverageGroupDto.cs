using System.ComponentModel.DataAnnotations;
using iOne.ProCoverageGroups;

namespace iOne.Product.ProCoverageGroups;

public class CreateProCoverageGroupDto
{
    [Required(ErrorMessage = "ProCoverageGroup:CodeRequired")]
    [StringLength(50, ErrorMessage = "ProCoverageGroup:CodeMaxLength")]
    [RegularExpression(@"^[A-Z0-9_]+$", ErrorMessage = "ProCoverageGroup:CodeInvalidFormat")]
    [Display(Name = "ProCoverageGroup:Code")]
    public string Code { get; set; } = null!;

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

