using System;
using System.ComponentModel.DataAnnotations;
using iOne.ResCarBrands;

namespace iOne.Master.ResCarModels;

public class CreateResCarModelDto
{
    [Required(ErrorMessage = "ResCarModel:CarBrandIdRequired")]
    [Display(Name = "ResCarModel:CarBrandId")]
    public Guid CarBrandId { get; set; }

    [Required(ErrorMessage = "ResCarModel:CodeRequired")]
    [StringLength(50, ErrorMessage = "ResCarModel:CodeMaxLength")]
    [RegularExpression(@"^[A-Z0-9_]+$", ErrorMessage = "ResCarModel:CodeInvalid")]
    [Display(Name = "ResCarModel:Code")]
    public string Code { get; set; } = null!;

    [Required(ErrorMessage = "ResCarModel:NameRequired")]
    [StringLength(250, ErrorMessage = "ResCarModel:NameMaxLength")]
    [Display(Name = "ResCarModel:Name")]
    public string Name { get; set; } = null!;

    [StringLength(500, ErrorMessage = "ResCarModel:DescriptionMaxLength")]
    [Display(Name = "ResCarModel:Description")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "ResCarModel:StatusRequired")]
    [Display(Name = "ResCarModel:Status")]
    public ResCarBrandStatus Status { get; set; } = ResCarBrandStatus.Active;
}



