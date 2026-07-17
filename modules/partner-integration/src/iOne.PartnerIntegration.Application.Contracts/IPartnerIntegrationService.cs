using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using iOne.Policies;
using Volo.Abp.DependencyInjection;

namespace iOne.PartnerIntegration;

public interface IPartnerIntegrationService : ITransientDependency
{
    Task<List<PartnerIntegrationResult>> IssuePolicyAsync(
        Policy policy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calls the partner's motor policy issuance API and returns a structured result.
    /// The caller (PolicyAppService.CreateAsync) must NOT catch exceptions from this method —
    /// any thrown exception will roll back the enclosing ABP UoW.
    /// </summary>
    Task<MotorPolicyIssueResult> IssueMotorPolicyAsync(
        Policy policy,
        PartnerIssueBillInfo? billInfo = null,
        PartnerIssueOwnerInfo? ownerInfo = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Queries the partner's API for an existing policy by its insurer-side reference and
    /// returns the raw partner response. Dispatches to the matching strategy based on partner code.
    /// </summary>
    Task<PartnerPolicyInquiryResult> InquiryPolicyAsync(
        Policy policy, CancellationToken cancellationToken = default);
}
