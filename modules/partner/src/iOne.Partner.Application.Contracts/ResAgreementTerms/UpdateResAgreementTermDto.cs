using System.ComponentModel.DataAnnotations;
using iOne.ResAgreementTerms;

namespace iOne.Partner.ResAgreementTerms;

public class UpdateResAgreementTermDto
{
    // ⚠️ QUAN TRỌNG: Không có Code property - Code không được phép sửa

    [Required(ErrorMessage = "Partner::ResAgreementTerm:NameRequired")]
    [StringLength(250, ErrorMessage = "Partner::ResAgreementTerm:NameMaxLength")]
    [Display(Name = "Partner::ResAgreementTerm:Name")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Partner::ResAgreementTerm:StatusRequired")]
    [Display(Name = "Partner::ResAgreementTerm:Status")]
    public ResAgreementTermStatus Status { get; set; }
}

