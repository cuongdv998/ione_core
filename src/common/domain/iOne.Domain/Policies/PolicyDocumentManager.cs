using System.Threading.Tasks;
using Volo.Abp.Domain.Services;

namespace iOne.Policies;

public class PolicyDocumentManager : DomainService
{
    protected IPolicyDocumentRepository Repository { get; }

    public PolicyDocumentManager(IPolicyDocumentRepository repository)
    {
        Repository = repository;
    }

    public virtual async Task CreateAsync(PolicyDocument policyDocument)
    {
        await Repository.InsertAsync(policyDocument);
    }
}
