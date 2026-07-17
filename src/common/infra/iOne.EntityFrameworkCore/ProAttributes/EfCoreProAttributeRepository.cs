using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using iOne.EntityFrameworkCore;
using iOne.ProAttributes;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace iOne.EntityFrameworkCore.ProAttributes;

public class EfCoreProAttributeRepository : EfCoreRepository<iOneDbContext, ProAttribute, Guid>,
    IProAttributeRepository
{
    public EfCoreProAttributeRepository(IDbContextProvider<iOneDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<bool> IsCodeExistsAsync(string code, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();
        var normalized = code.ToLowerInvariant();
        query = query.Where(x => x.Code.ToLower() == normalized);

        if (excludeId.HasValue)
        {
            query = query.Where(x => x.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<ProAttribute?> FindByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();
        var normalized = code.ToLowerInvariant();
        return await query.FirstOrDefaultAsync(x => x.Code.ToLower() == normalized, cancellationToken);
    }
}
