using System.ComponentModel.DataAnnotations;
using iOne.ResCountries;

namespace iOne.Master.ResCountries;

public class CreateResCountryDto
{
    [Required(ErrorMessage = "ResCountry:CodeRequired")]
    [StringLength(25, ErrorMessage = "ResCountry:CodeMaxLength")]
    [RegularExpression(@"^[A-Z0-9_]+$", ErrorMessage = "ResCountry:CodeInvalid")]
    [Display(Name = "ResCountry:Code")]
    public string Code { get; set; } = null!;

    [Required(ErrorMessage = "ResCountry:NameRequired")]
    [StringLength(250, ErrorMessage = "ResCountry:NameMaxLength")]
    [Display(Name = "ResCountry:Name")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "ResCountry:StatusRequired")]
    [Display(Name = "ResCountry:Status")]
    public ResCountryStatus Status { get; set; } = ResCountryStatus.Active;
}

