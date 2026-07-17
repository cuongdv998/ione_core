using System;
using System.ComponentModel.DataAnnotations;

namespace iOne.Partner.ResPartners;

public class CreateResPartnerAgreementDto
{
    [Required(ErrorMessage = "Partner:ResPartnerAgreement:AgreementTermIdRequired")]
    [Display(Name = "Partner:ResPartnerAgreement:AgreementTermId")]
    public Guid AgreementTermId { get; set; }

    [Required(ErrorMessage = "Partner:ResPartnerAgreement:ValueRequired")]
    [StringLength(50, ErrorMessage = "Partner:ResPartnerAgreement:ValueMaxLength")]
    [Display(Name = "Partner:ResPartnerAgreement:Value")]
    public string Value { get; set; } = null!;

    [Display(Name = "Partner:ResPartnerAgreement:EffectDate")]
    public DateTime? EffectDate { get; set; }

    [Required(ErrorMessage = "Partner:ResPartnerAgreement:ExpireDateRequired")]
    [Display(Name = "Partner:ResPartnerAgreement:ExpireDate")]
    public DateTime ExpireDate { get; set; }
}

