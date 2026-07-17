using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using iOne.EntityFrameworkCore;
using iOne.ProProducts;

namespace iOne.EntityFrameworkCore.ProProducts;

public class EfCoreProProductRepository : EfCoreRepository<iOneDbContext, ProProduct, Guid>, IProProductRepository
{
    public EfCoreProProductRepository(IDbContextProvider<iOneDbContext> dbContextProvider)
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

    public async Task<bool> AnyProductReferencesPlanDefinitionAsync(Guid planDefinitionId)
    {
        var query = await GetQueryableAsync();
        return await query.AnyAsync(x => x.PlanDefinitionId == planDefinitionId);
    }
}
