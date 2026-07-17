using System.ComponentModel.DataAnnotations;
using iOne.ResCurrencies;

namespace iOne.Master.ResCurrencies;

public class CreateResCurrencyDto
{
    [Required(ErrorMessage = "ResCurrency:CodeRequired")]
    [StringLength(50, ErrorMessage = "ResCurrency:CodeMaxLength")]
    [RegularExpression(@"^[A-Z0-9_]+$", ErrorMessage = "ResCurrency:CodeInvalid")]
    [Display(Name = "ResCurrency:Code")]
    public string Code { get; set; } = null!;

    [Required(ErrorMessage = "ResCurrency:NameRequired")]
    [StringLength(250, ErrorMessage = "ResCurrency:NameMaxLength")]
    [Display(Name = "ResCurrency:Name")]
    public string Name { get; set; } = null!;

    [StringLength(500, ErrorMessage = "ResCurrency:DescriptionMaxLength")]
    [Display(Name = "ResCurrency:Description")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "ResCurrency:StatusRequired")]
    [Display(Name = "ResCurrency:Status")]
    public ResCurrencyStatus Status { get; set; } = ResCurrencyStatus.Active;
}
