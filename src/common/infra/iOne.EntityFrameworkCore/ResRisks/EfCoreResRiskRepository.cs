using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using iOne.EntityFrameworkCore;
using iOne.ResRisks;

namespace iOne.EntityFrameworkCore.ResRisks;

public class EfCoreResRiskRepository : EfCoreRepository<iOneDbContext, ResRisk, Guid>, IResRiskRepository
{
    public EfCoreResRiskRepository(IDbContextProvider<iOneDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<bool> IsCodeExistsAsync(string code, Guid? excludeId = null)
    {
        var query = await GetQueryableAsync();
        query = query.Where(x => x.Code == code);

        if (excludeId.HasValue)
        {
            query = query.Where(x => x.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<bool> IsObjectTypeInUseAsync(Guid objectTypeId)
    {
        var query = await GetQueryableAsync();
        return await query.AnyAsync(x => x.ObjectTypeId == objectTypeId);
    }
}

