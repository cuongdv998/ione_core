using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using iOne.EntityFrameworkCore;
using iOne.BusinessFlows;

namespace iOne.EntityFrameworkCore.BusinessFlows;

public class EfCoreBusinessFlowRepository : EfCoreRepository<iOneDbContext, BusinessFlow, Guid>, IBusinessFlowRepository
{
    public EfCoreBusinessFlowRepository(IDbContextProvider<iOneDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<bool> ExistsOverlapAsync(
        Guid? organizationId,
        Guid? insurerId,
        string businessCode,
        DateTime effectDate,
        DateTime? expireDate,
        Guid? excludeId = null)
    {
        var end = expireDate ?? DateTime.MaxValue;
        var query = await GetQueryableAsync();
        query = query.Where(x => !x.IsDeleted
            && x.OrganizationId == organizationId
            && x.InsurerId == insurerId
            && x.BusinessCode == businessCode);

        if (excludeId.HasValue)
        {
            query = query.Where(x => x.Id != excludeId.Value);
        }

        var list = await query.ToListAsync();
        foreach (var x in list)
        {
            var xEnd = x.ExpireDate ?? DateTime.MaxValue;
            if (effectDate <= xEnd && x.EffectDate <= end)
            {
                return true;
            }
        }

        return false;
    }
}
