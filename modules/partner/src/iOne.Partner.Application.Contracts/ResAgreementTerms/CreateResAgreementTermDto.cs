using System.ComponentModel.DataAnnotations;
using iOne.ResAgreementTerms;

namespace iOne.Partner.ResAgreementTerms;

public class CreateResAgreementTermDto
{
    [Required(ErrorMessage = "Partner::ResAgreementTerm:CodeRequired")]
    [StringLength(25, ErrorMessage = "Partner::ResAgreementTerm:CodeMaxLength")]
    [RegularExpression(@"^[A-Z0-9_]+$", ErrorMessage = "Partner::ResAgreementTerm:CodeInvalid")]
    [Display(Name = "Partner::ResAgreementTerm:Code")]
    public string Code { get; set; } = null!;

    [Required(ErrorMessage = "Partner::ResAgreementTerm:NameRequired")]
    [StringLength(250, ErrorMessage = "Partner::ResAgreementTerm:NameMaxLength")]
    [Display(Name = "Partner::ResAgreementTerm:Name")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Partner::ResAgreementTerm:StatusRequired")]
    [Display(Name = "Partner::ResAgreementTerm:Status")]
    public ResAgreementTermStatus Status { get; set; } = ResAgreementTermStatus.Active;
}

