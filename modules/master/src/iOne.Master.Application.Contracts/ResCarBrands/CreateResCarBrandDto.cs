using System.ComponentModel.DataAnnotations;
using iOne.ResCarBrands;

namespace iOne.Master.ResCarBrands;

public class CreateResCarBrandDto
{
    [Required(ErrorMessage = "ResCarBrand:CodeRequired")]
    [StringLength(50, ErrorMessage = "ResCarBrand:CodeMaxLength")]
    [RegularExpression(@"^[A-Z0-9_]+$", ErrorMessage = "ResCarBrand:CodeInvalid")]
    [Display(Name = "ResCarBrand:Code")]
    public string Code { get; set; } = null!;

    [Required(ErrorMessage = "ResCarBrand:NameRequired")]
    [StringLength(250, ErrorMessage = "ResCarBrand:NameMaxLength")]
    [Display(Name = "ResCarBrand:Name")]
    public string Name { get; set; } = null!;

    [StringLength(500, ErrorMessage = "ResCarBrand:DescriptionMaxLength")]
    [Display(Name = "ResCarBrand:Description")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "ResCarBrand:StatusRequired")]
    [Display(Name = "ResCarBrand:Status")]
    public ResCarBrandStatus Status { get; set; } = ResCarBrandStatus.Active;
}



