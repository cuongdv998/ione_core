using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using iOne.EntityFrameworkCore;
using iOne.ProCoverageLevelBases;
using ProCoverageLevelBasisEntity = iOne.ProCoverageLevelBases.ProCoverageLevelBasis;

namespace iOne.EntityFrameworkCore.ProCoverageLevelBasis;

public class EfCoreProCoverageLevelBasisRepository : EfCoreRepository<iOneDbContext, ProCoverageLevelBasisEntity, Guid>, IProCoverageLevelBasisRepository
{
    public EfCoreProCoverageLevelBasisRepository(IDbContextProvider<iOneDbContext> dbContextProvider)
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
}
