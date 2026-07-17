using System;
using iOne.EntityFrameworkCore;
using iOne.ResObjectItemDepreciations;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
namespace iOne.EntityFrameworkCore.ResObjectItemDepreciations;

public class ResObjectItemDepreciationRepository : EfCoreRepository<iOneDbContext, ResObjectItemDepreciation, Guid>, IResObjectItemDepreciationRepository
{
    public ResObjectItemDepreciationRepository(IDbContextProvider<iOneDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<bool> IsCarGroupInUseAsync(Guid carGroupId)
    {
        var dbContext = await GetDbContextAsync();
        return await dbContext.Set<ResObjectItemDepreciation>().AnyAsync(x => x.CarGroupId == carGroupId);
    }

    public async Task<bool> IsObjectTypeItemInUseAsync(Guid objectTypeItemId)
    {
        var dbContext = await GetDbContextAsync();
        return await dbContext.Set<ResObjectItemDepreciation>().AnyAsync(x => x.ObjectTypeItemId == objectTypeItemId);
    }
}
