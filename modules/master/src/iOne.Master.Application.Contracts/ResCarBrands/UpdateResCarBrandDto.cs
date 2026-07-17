using System.ComponentModel.DataAnnotations;
using iOne.ResCarBrands;

namespace iOne.Master.ResCarBrands;

public class UpdateResCarBrandDto
{
    // ⚠️ QUAN TRỌNG: Không có Code property - Code không được phép sửa

    [Required(ErrorMessage = "ResCarBrand:NameRequired")]
    [StringLength(250, ErrorMessage = "ResCarBrand:NameMaxLength")]
    [Display(Name = "ResCarBrand:Name")]
    public string Name { get; set; } = null!;

    [StringLength(500, ErrorMessage = "ResCarBrand:DescriptionMaxLength")]
    [Display(Name = "ResCarBrand:Description")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "ResCarBrand:StatusRequired")]
    [Display(Name = "ResCarBrand:Status")]
    public ResCarBrandStatus Status { get; set; }
}



