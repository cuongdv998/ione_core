using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace iOne.Policies;

public class PolicyVersionManager : DomainService
{
    protected IPolicyVersionRepository Repository { get; }

    public PolicyVersionManager(IPolicyVersionRepository repository)
    {
        Repository = repository;
    }

    public virtual async Task CreateAsync(PolicyVersion policyVersion)
    {
        await Repository.InsertAsync(policyVersion);
    }

    public virtual async Task UpdateAsync(
        PolicyVersion policyVersion,
        decimal version,
        string status,
        DateTime? expireDate = null,
        DateTime? orgExpireDate = null)
    {
        policyVersion.UpdateVersion(version);
        policyVersion.UpdateStatus(status);
        
        if (expireDate.HasValue)
        {
            policyVersion.UpdateExpireDate(expireDate.Value);
        }
        
        if (orgExpireDate.HasValue)
        {
            policyVersion.UpdateOrgExpireDate(orgExpireDate.Value);
        }
        
        await Repository.UpdateAsync(policyVersion);
    }
}
