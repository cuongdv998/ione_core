using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using iOne.EntityFrameworkCore;
using iOne.ResCarGroups;

namespace iOne.EntityFrameworkCore.ResCarGroups;

public class EfCoreResCarGroupRepository : EfCoreRepository<iOneDbContext, ResCarGroup, Guid>, IResCarGroupRepository
{
    public EfCoreResCarGroupRepository(IDbContextProvider<iOneDbContext> dbContextProvider)
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

    public async Task<bool> HasGroupsForLineAsync(Guid carLineId)
    {
        var query = await GetQueryableAsync();
        return await query.AnyAsync(x => x.CarLineId == carLineId);
    }
}
