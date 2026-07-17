using System.ComponentModel.DataAnnotations;
using iOne.ResPaymentMethods;

namespace iOne.Master.ResPaymentMethods;

public class UpdateResPaymentMethodDto
{
    // ⚠️ QUAN TRỌNG: Không có Code property - Code không được phép sửa

    [Required(ErrorMessage = "ResPaymentMethod:NameRequired")]
    [StringLength(250, ErrorMessage = "ResPaymentMethod:NameMaxLength")]
    [Display(Name = "ResPaymentMethod:Name")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "ResPaymentMethod:StatusRequired")]
    [Display(Name = "ResPaymentMethod:Status")]
    public ResPaymentMethodStatus Status { get; set; }
}
