using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using iOne.ProProductCategorys;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace iOne.EntityFrameworkCore.ProProductCategorys;

public class EfCoreProProductCategoryRepository : EfCoreRepository<iOneDbContext, ProProductCategory, Guid>,
    IProProductCategoryRepository
{
    public EfCoreProProductCategoryRepository(IDbContextProvider<iOneDbContext> dbContextProvider)
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

    public virtual async Task<ProProductCategory?> FindByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();
        return await query.FirstOrDefaultAsync(x => x.Code == code, cancellationToken);
    }

    public virtual async Task<List<ProProductCategory>> GetChildrenAsync(Guid parentId, CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();
        return await query.Where(x => x.ParentId == parentId).ToListAsync(cancellationToken);
    }

    public virtual async Task<bool> HasChildrenAsync(Guid parentId, CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();
        return await query.AnyAsync(x => x.ParentId == parentId, cancellationToken);
    }
}
