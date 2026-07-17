using System.ComponentModel.DataAnnotations;
using iOne.ResCarBrands;

namespace iOne.Master.ResCarModels;

public class UpdateResCarModelDto
{
    // ⚠️ QUAN TRỌNG: Không có Code property - Code không được phép sửa
    // ⚠️ QUAN TRỌNG: Không có CarBrandId property - CarBrandId không được phép sửa

    [Required(ErrorMessage = "ResCarModel:NameRequired")]
    [StringLength(250, ErrorMessage = "ResCarModel:NameMaxLength")]
    [Display(Name = "ResCarModel:Name")]
    public string Name { get; set; } = null!;

    [StringLength(500, ErrorMessage = "ResCarModel:DescriptionMaxLength")]
    [Display(Name = "ResCarModel:Description")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "ResCarModel:StatusRequired")]
    [Display(Name = "ResCarModel:Status")]
    public ResCarBrandStatus Status { get; set; }
}



