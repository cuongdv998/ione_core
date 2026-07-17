using System;
using Volo.Abp.Application.Dtos;

namespace iOne.Policy.PolicyCertificates;

public class GetPolicyCertificatesInput : PagedAndSortedResultRequestDto
{
    public Guid? PolicyId { get; set; }
    
    public Guid? PolicyVersionId { get; set; }
    
    public string? CertificateNo { get; set; }
}
