using System.ComponentModel.DataAnnotations;
using iOne.ResPaymentMethods;

namespace iOne.Master.ResPaymentMethods;

public class CreateResPaymentMethodDto
{
    [Required(ErrorMessage = "ResPaymentMethod:CodeRequired")]
    [StringLength(25, ErrorMessage = "ResPaymentMethod:CodeMaxLength")]
    [RegularExpression(@"^[A-Z0-9_]+$", ErrorMessage = "ResPaymentMethod:CodeInvalid")]
    [Display(Name = "ResPaymentMethod:Code")]
    public string Code { get; set; } = null!;

    [Required(ErrorMessage = "ResPaymentMethod:NameRequired")]
    [StringLength(250, ErrorMessage = "ResPaymentMethod:NameMaxLength")]
    [Display(Name = "ResPaymentMethod:Name")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "ResPaymentMethod:StatusRequired")]
    [Display(Name = "ResPaymentMethod:Status")]
    public ResPaymentMethodStatus Status { get; set; } = ResPaymentMethodStatus.Active;
}
