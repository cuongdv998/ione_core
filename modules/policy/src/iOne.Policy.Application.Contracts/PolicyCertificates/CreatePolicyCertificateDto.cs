using System;
using System.ComponentModel.DataAnnotations;

namespace iOne.Policy.PolicyCertificates;

public class CreatePolicyCertificateDto
{
    [Required(ErrorMessage = "PolicyCertificate:PolicyIdRequired")]
    [Display(Name = "PolicyCertificate:PolicyId")]
    public Guid PolicyId { get; set; }

    [Required(ErrorMessage = "PolicyCertificate:PolicyVersionIdRequired")]
    [Display(Name = "PolicyCertificate:PolicyVersionId")]
    public Guid PolicyVersionId { get; set; }

    [StringLength(50, ErrorMessage = "PolicyCertificate:CertificateNoMaxLength")]
    [Display(Name = "PolicyCertificate:CertificateNo")]
    public string? CertificateNo { get; set; }

    [StringLength(250, ErrorMessage = "PolicyCertificate:UrlMaxLength")]
    [Display(Name = "PolicyCertificate:Url")]
    public string? Url { get; set; }
}
