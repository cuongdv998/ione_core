using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace iOne.Policy.Policies;

public interface IPolicyPartnerAppService : IApplicationService
{
    Task<PartnerCreatePolicyResultDto> PartnerCreatePolicyAsync(PartnerCreatePolicyInput input);

    /// <summary>
    /// Queries the partner's API for the policy identified by <paramref name="policyId"/> and
    /// returns the raw partner response. The policy's <c>InsurerPolicyNo</c> is used as the
    /// partner-side reference key.
    /// </summary>
    Task<PartnerPolicyInquiryResultDto> PartnerPolicyInquiryAsync(string policyId);
}
