using System.ComponentModel.DataAnnotations;
using iOne.ResPaymentTypes;

namespace iOne.Master.ResPaymentTypes;

public class CreateResPaymentTypeDto
{
    [Required(ErrorMessage = "ResPaymentType:CodeRequired")]
    [StringLength(50, ErrorMessage = "ResPaymentType:CodeMaxLength")]
    [RegularExpression(@"^[A-Z0-9_]+$", ErrorMessage = "ResPaymentType:CodeInvalid")]
    [Display(Name = "ResPaymentType:Code")]
    public string Code { get; set; } = null!;

    [Required(ErrorMessage = "ResPaymentType:NameRequired")]
    [StringLength(250, ErrorMessage = "ResPaymentType:NameMaxLength")]
    [Display(Name = "ResPaymentType:Name")]
    public string Name { get; set; } = null!;

    [StringLength(500, ErrorMessage = "ResPaymentType:DescriptionMaxLength")]
    [Display(Name = "ResPaymentType:Description")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "ResPaymentType:StatusRequired")]
    [Display(Name = "ResPaymentType:Status")]
    public ResPaymentTypeStatus Status { get; set; } = ResPaymentTypeStatus.Active;
}
