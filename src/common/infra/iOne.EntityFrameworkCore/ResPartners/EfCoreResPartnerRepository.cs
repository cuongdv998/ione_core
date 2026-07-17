using System;
using System.Linq;
using System.Threading.Tasks;
using iOne.ResPartners;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace iOne.EntityFrameworkCore.ResPartners;

public class EfCoreResPartnerRepository : EfCoreRepository<iOneDbContext, ResPartner, Guid>, IResPartnerRepository
{
    public EfCoreResPartnerRepository(IDbContextProvider<iOneDbContext> dbContextProvider)
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
        return await dbSet.AnyAsync(x => !x.IsDeleted && (x.ProvinceId == provinceId || x.InvoiceProvinceId == provinceId));
    }
    
    public async Task<bool> AnyByWardIdAsync(Guid wardId)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet.AnyAsync(x => !x.IsDeleted && (x.WardId == wardId || x.InvoiceWardId == wardId));
    }

    public async Task<ResPartner?> FindByCodeAsync(string code)
    {
        var query = await GetQueryableAsync();
        return await query.FirstOrDefaultAsync(x => x.Code == code);
    }
}

