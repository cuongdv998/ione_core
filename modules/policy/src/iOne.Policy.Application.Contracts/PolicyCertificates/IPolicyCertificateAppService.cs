using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace iOne.Policy.PolicyCertificates;

public interface IPolicyCertificateAppService : ICrudAppService<
    PolicyCertificateDto,
    Guid,
    GetPolicyCertificatesInput,
    CreatePolicyCertificateDto,
    UpdatePolicyCertificateDto>
{
    Task<PolicyCertificateDto> GetUrlByPolicyIdAsync(Guid policyId, Guid? policyVersionId = null);
}
