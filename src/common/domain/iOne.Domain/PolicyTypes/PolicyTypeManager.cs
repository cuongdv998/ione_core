using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace iOne.PolicyTypes;

public class PolicyTypeManager : DomainService
{
    protected IPolicyTypeRepository Repository { get; }

    public PolicyTypeManager(IPolicyTypeRepository repository)
    {
        Repository = repository;
    }

    public virtual async Task CreateAsync(PolicyType policyType)
    {
        // Check code uniqueness
        if (await Repository.IsCodeExistsAsync(policyType.Code))
        {
            throw new BusinessException("Policy:PolicyType:CodeExists")
                .WithData("Code", policyType.Code);
        }

        await Repository.InsertAsync(policyType);
    }

    public virtual async Task UpdateAsync(PolicyType policyType, string name, string? description, PolicyTypeStatus status)
    {
        policyType.UpdateName(name);
        policyType.UpdateDescription(description);
        policyType.UpdateStatus(status);
        await Repository.UpdateAsync(policyType);
    }
}
