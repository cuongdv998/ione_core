using System.ComponentModel.DataAnnotations;
using iOne.ResCountries;

namespace iOne.Master.ResCountries;

public class UpdateResCountryDto
{
    // ⚠️ QUAN TRỌNG: Không có Code property - Code không được phép sửa

    [Required(ErrorMessage = "ResCountry:NameRequired")]
    [StringLength(250, ErrorMessage = "ResCountry:NameMaxLength")]
    [Display(Name = "ResCountry:Name")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "ResCountry:StatusRequired")]
    [Display(Name = "ResCountry:Status")]
    public ResCountryStatus Status { get; set; }
}

