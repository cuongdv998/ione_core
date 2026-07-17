using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using iOne.ApiKeys;
using iOne.EntityFrameworkCore;

namespace iOne.EntityFrameworkCore.ApiKeys;

public class EfCoreApiKeyRepository : EfCoreRepository<iOneDbContext, ApiKey, Guid>, IApiKeyRepository
{
    public EfCoreApiKeyRepository(IDbContextProvider<iOneDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<ApiKey?> FindByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();
        return await query.FirstOrDefaultAsync(x => x.Prefix == prefix, cancellationToken);
    }

    public async Task<List<ApiKey>> GetListByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();
        return await query
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreationTime)
            .ToListAsync(cancellationToken);
    }
}
