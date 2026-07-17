using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using iOne.EntityFrameworkCore;
using iOne.ProProductPlanDefinitions;

namespace iOne.EntityFrameworkCore.ProProductPlanDefinitions;

public class EfCoreProProductPlanDefinitionRepository : EfCoreRepository<iOneDbContext, ProProductPlanDefinition, Guid>, IProProductPlanDefinitionRepository
{
    public EfCoreProProductPlanDefinitionRepository(IDbContextProvider<iOneDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<bool> IsCodeExistsAsync(string planCode, Guid? excludeId = null)
    {
        var query = await GetQueryableAsync();
        query = query.Where(x => x.PlanCode == planCode);

        if (excludeId.HasValue)
        {
            query = query.Where(x => x.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }
}
