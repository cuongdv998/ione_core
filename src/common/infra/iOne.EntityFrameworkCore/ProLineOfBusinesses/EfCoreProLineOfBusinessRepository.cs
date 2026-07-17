using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using iOne.ProLineOfBusinesses;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace iOne.EntityFrameworkCore.ProLineOfBusinesses;

public class EfCoreProLineOfBusinessRepository : EfCoreRepository<iOneDbContext, ProLineOfBusiness, Guid>,
    IProLineOfBusinessRepository
{
    public EfCoreProLineOfBusinessRepository(IDbContextProvider<iOneDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public virtual async Task<bool> IsCodeExistsAsync(string code, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();
        query = query.Where(x => x.Code == code);

        if (excludeId.HasValue)
        {
            query = query.Where(x => x.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public virtual async Task<ProLineOfBusiness?> FindByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();
        return await query.FirstOrDefaultAsync(x => x.Code == code, cancellationToken);
    }

    public virtual async Task<List<ProLineOfBusiness>> GetChildrenAsync(Guid parentId, CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();
        return await query.Where(x => x.ParentId == parentId).ToListAsync(cancellationToken);
    }
}




