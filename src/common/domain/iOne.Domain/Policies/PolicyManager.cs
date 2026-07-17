using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace iOne.Policies;

public class PolicyManager : DomainService
{
    protected IPolicyRepository Repository { get; }

    public PolicyManager(IPolicyRepository repository)
    {
        Repository = repository;
    }

    public virtual async Task CreateAsync(Policy policy)
    {
        // Check policy number uniqueness
        if (await Repository.IsPolicyNoExistsAsync(policy.PolicyNo))
        {
            throw new BusinessException("Policy:Policy:PolicyNoExists")
                .WithData("PolicyNo", policy.PolicyNo);
        }

        await Repository.InsertAsync(policy);
    }

    public virtual async Task UpdateAsync(Policy policy)
    {
        await Repository.UpdateAsync(policy);
    }
}
