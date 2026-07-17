using System;
using System.Threading.Tasks;
using iOne.EntityFrameworkCore;
using iOne.ResPartnerTypes;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace iOne.EntityFrameworkCore.ResPartnerTypes;

public class EfCoreResPartnerTypeRepository
    : EfCoreRepository<iOneDbContext, ResPartnerType, Guid>,
      IResPartnerTypeRepository
{
    public EfCoreResPartnerTypeRepository(
        IDbContextProvider<iOneDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<bool> IsCodeExistsAsync(string code)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet.AnyAsync(x => x.Code == code);
    }
}

