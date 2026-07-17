using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace iOne.PolicyContracts;

public class PolicyContractManager : DomainService
{
    protected IPolicyContractRepository Repository { get; }

    public PolicyContractManager(IPolicyContractRepository repository)
    {
        Repository = repository;
    }

    public virtual async Task CreateAsync(PolicyContract policyContract)
    {
        // Check code uniqueness
        if (await Repository.IsCodeExistsAsync(policyContract.Code))
        {
            throw new BusinessException("Policy:PolicyContract:CodeExists")
                .WithData("Code", policyContract.Code);
        }

        await Repository.InsertAsync(policyContract);
    }

    public virtual async Task UpdateAsync(PolicyContract policyContract)
    {
        await Repository.UpdateAsync(policyContract);
    }
}
