using System.ComponentModel.DataAnnotations;
using iOne.ResCurrencies;

namespace iOne.Master.ResCurrencies;

public class UpdateResCurrencyDto
{
    // ⚠️ QUAN TRỌNG: Không có Code property - Code không được phép sửa

    [Required(ErrorMessage = "ResCurrency:NameRequired")]
    [StringLength(250, ErrorMessage = "ResCurrency:NameMaxLength")]
    [Display(Name = "ResCurrency:Name")]
    public string Name { get; set; } = null!;

    [StringLength(500, ErrorMessage = "ResCurrency:DescriptionMaxLength")]
    [Display(Name = "ResCurrency:Description")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "ResCurrency:StatusRequired")]
    [Display(Name = "ResCurrency:Status")]
    public ResCurrencyStatus Status { get; set; }
}
