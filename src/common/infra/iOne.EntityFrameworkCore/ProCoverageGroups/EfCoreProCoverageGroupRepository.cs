using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using iOne.ProCoverageGroups;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace iOne.EntityFrameworkCore.ProCoverageGroups;

public class EfCoreProCoverageGroupRepository : EfCoreRepository<iOneDbContext, ProCoverageGroup, Guid>,
    IProCoverageGroupRepository
{
    public EfCoreProCoverageGroupRepository(IDbContextProvider<iOneDbContext> dbContextProvider)
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

    public virtual async Task<ProCoverageGroup?> FindByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();
        return await query.FirstOrDefaultAsync(x => x.Code == code, cancellationToken);
    }
}

