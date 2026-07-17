using System.ComponentModel.DataAnnotations;

namespace iOne.Policy.PolicyCertificates;

public class UpdatePolicyCertificateDto
{
    [StringLength(50, ErrorMessage = "PolicyCertificate:CertificateNoMaxLength")]
    [Display(Name = "PolicyCertificate:CertificateNo")]
    public string? CertificateNo { get; set; }

    [StringLength(250, ErrorMessage = "PolicyCertificate:UrlMaxLength")]
    [Display(Name = "PolicyCertificate:Url")]
    public string? Url { get; set; }
}
