using System;
using System.ComponentModel.DataAnnotations;

namespace iOne.Partner.ResPartners;

public class UpdateResPartnerAgreementDto
{
    /// <summary>
    /// Id c?a agreement. N?u null ho?c Guid.Empty thì s? t?o m?i, ng??c l?i s? update.
    /// </summary>
    [Display(Name = "Partner:ResPartnerAgreement:Id")]
    public Guid? Id { get; set; }

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

