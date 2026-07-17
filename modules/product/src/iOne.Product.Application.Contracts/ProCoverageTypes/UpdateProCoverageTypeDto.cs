using System.ComponentModel.DataAnnotations;
using iOne.ProCoverageTypes;

namespace iOne.Product.ProCoverageTypes;

public class UpdateProCoverageTypeDto
{
    // ⚠️ QUAN TRỌNG: Không có Code - Code không được phép sửa

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

