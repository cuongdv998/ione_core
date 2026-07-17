using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace iOne.PartnerIntegration.Vni;

public interface IVniApiClient : ITransientDependency
{
    Task<VniIssueResult> IssueCarTndsPolicyAsync(VniCarTndsIssueRequest request, CancellationToken cancellationToken = default);

    Task<VniIssueResult> IssueCarVcxPolicyAsync(VniCarVcxIssueRequest request, CancellationToken cancellationToken = default);
}
