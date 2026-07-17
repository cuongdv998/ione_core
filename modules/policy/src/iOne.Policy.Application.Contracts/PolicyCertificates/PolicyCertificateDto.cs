using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace iOne.Policy.PolicyCertificates;

public class PolicyCertificateDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "PolicyCertificate:PolicyId")]
    public Guid PolicyId { get; set; }

    [Display(Name = "PolicyCertificate:PolicyVersionId")]
    public Guid PolicyVersionId { get; set; }

    [Display(Name = "PolicyCertificate:CertificateNo")]
    public string? CertificateNo { get; set; }

    [Display(Name = "PolicyCertificate:Url")]
    public string? Url { get; set; }
}
