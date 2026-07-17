using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using iOne.EntityFrameworkCore;
using iOne.ResOrganizationTypes;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace iOne.EntityFrameworkCore.ResOrganizationTypes;

public class EfCoreResOrganizationTypeRepository
    : EfCoreRepository<iOneDbContext, ResOrganizationType, Guid>,
      IResOrganizationTypeRepository
{
    public EfCoreResOrganizationTypeRepository(
        IDbContextProvider<iOneDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<bool> IsCodeExistsAsync(string code, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        var query = dbSet.Where(x => x.Code == code);
        
        if (excludeId.HasValue)
        {
            query = query.Where(x => x.Id != excludeId.Value);
        }
        
        return await query.AnyAsync(cancellationToken);
    }
}

