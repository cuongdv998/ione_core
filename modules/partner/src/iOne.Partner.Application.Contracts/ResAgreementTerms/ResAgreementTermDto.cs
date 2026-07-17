using System;
using System.ComponentModel.DataAnnotations;
using iOne.ResAgreementTerms;
using Volo.Abp.Application.Dtos;

namespace iOne.Partner.ResAgreementTerms;

public class ResAgreementTermDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "Partner::ResAgreementTerm:Code")]
    public string Code { get; set; } = null!;

    [Display(Name = "Partner::ResAgreementTerm:Name")]
    public string Name { get; set; } = null!;

    [Display(Name = "Partner::ResAgreementTerm:Status")]
    public ResAgreementTermStatus Status { get; set; }
}

