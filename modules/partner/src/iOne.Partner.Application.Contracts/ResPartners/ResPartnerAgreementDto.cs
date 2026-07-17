using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace iOne.Partner.ResPartners;

public class ResPartnerAgreementDto : AuditedEntityDto<Guid>
{
    [Display(Name = "Partner:ResPartnerAgreement:PartnerId")]
    public Guid PartnerId { get; set; }

    [Display(Name = "Partner:ResPartnerAgreement:AgreementTermId")]
    public Guid AgreementTermId { get; set; }

    [Display(Name = "Partner:ResPartnerAgreement:AgreementTermName")]
    public string AgreementTermName { get; set; } = null!;

    [Display(Name = "Partner:ResPartnerAgreement:Value")]
    public string Value { get; set; } = null!;

    [Display(Name = "Partner:ResPartnerAgreement:EffectDate")]
    public DateTime? EffectDate { get; set; }

    [Display(Name = "Partner:ResPartnerAgreement:ExpireDate")]
    public DateTime ExpireDate { get; set; }
}

