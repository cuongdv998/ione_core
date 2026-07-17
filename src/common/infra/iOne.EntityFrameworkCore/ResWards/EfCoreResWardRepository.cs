using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using iOne.EntityFrameworkCore;
using iOne.ResWards;

namespace iOne.EntityFrameworkCore.ResWards;

public class EfCoreResWardRepository : EfCoreRepository<iOneDbContext, ResWard, Guid>, IResWardRepository
{
    public EfCoreResWardRepository(IDbContextProvider<iOneDbContext> dbContextProvider)
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

    public async Task<bool> AnyByProvinceIdAsync(Guid provinceId)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet.AnyAsync(x => !x.IsDeleted && x.ProvinceId == provinceId);
    }

    public async Task<ResWard?> FindByCodeAsync(string code)
    {
        var query = await GetQueryableAsync();
        return await query.FirstOrDefaultAsync(x => x.Code == code);
    }
}

