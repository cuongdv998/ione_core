using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace iOne.Policy.Policies;

public interface IPolicyMotorPartnerAppService : IApplicationService
{
    /// <summary>
    /// Calls the partner's motorbike policy issuance API for the given policy,
    /// persists the returned identifiers, and creates an AccountPaymentRequest on success.
    /// Throws a BusinessException when the policy's LOB is not the motorbike LOB.
    /// </summary>
    Task<PartnerMotorPolicyIssueResultDto> IssueMotorPolicyAsync(PartnerMotorPolicyIssueInput input);
}
