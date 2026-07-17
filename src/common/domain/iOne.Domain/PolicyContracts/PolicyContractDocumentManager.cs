using System.Threading.Tasks;
using Volo.Abp.Domain.Services;

namespace iOne.PolicyContracts;

public class PolicyContractDocumentManager : DomainService
{
    protected IPolicyContractDocumentRepository Repository { get; }

    public PolicyContractDocumentManager(IPolicyContractDocumentRepository repository)
    {
        Repository = repository;
    }

    public virtual async Task CreateAsync(PolicyContractDocument policyContractDocument)
    {
        await Repository.InsertAsync(policyContractDocument);
    }
}
