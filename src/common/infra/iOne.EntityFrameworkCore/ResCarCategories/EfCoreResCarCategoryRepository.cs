using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using iOne.EntityFrameworkCore;
using iOne.ResCarCategories;

namespace iOne.EntityFrameworkCore.ResCarCategories;

public class EfCoreResCarCategoryRepository : EfCoreRepository<iOneDbContext, ResCarCategory, Guid>, IResCarCategoryRepository
{
    public EfCoreResCarCategoryRepository(IDbContextProvider<iOneDbContext> dbContextProvider)
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

    public async Task<bool> HasCategoriesForBrandAsync(Guid carBrandId)
    {
        var query = await GetQueryableAsync();
        return await query.AnyAsync(x => x.CarBrandId == carBrandId);
    }

    public async Task<bool> HasCategoriesForModelAsync(Guid carModelId)
    {
        var query = await GetQueryableAsync();
        return await query.AnyAsync(x => x.CarModelId == carModelId);
    }

    public async Task<bool> HasCategoriesForLineAsync(Guid carLineId)
    {
        var query = await GetQueryableAsync();
        return await query.AnyAsync(x => x.CarLineId == carLineId);
    }
}


