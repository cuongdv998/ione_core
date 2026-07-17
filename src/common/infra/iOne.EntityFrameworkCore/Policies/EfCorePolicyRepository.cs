using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using iOne.EntityFrameworkCore;
using iOne.Policies;

namespace iOne.EntityFrameworkCore.Policies;

public class EfCorePolicyRepository : EfCoreRepository<iOneDbContext, Policy, Guid>, IPolicyRepository
{
    public EfCorePolicyRepository(IDbContextProvider<iOneDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<bool> IsPolicyNoExistsAsync(string policyNo, Guid? excludeId = null)
    {
        var query = await GetQueryableAsync();
        query = query.Where(x => x.PolicyNo == policyNo);

        if (excludeId.HasValue)
        {
            query = query.Where(x => x.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }
}
