using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using iOne.EntityFrameworkCore;
using iOne.ResReasonGroups;
using iOne.ResReasons;

namespace iOne.EntityFrameworkCore.ResReasonGroups;

public class EfCoreResReasonGroupRepository : EfCoreRepository<iOneDbContext, ResReasonGroup, Guid>, IResReasonGroupRepository
{
    public EfCoreResReasonGroupRepository(IDbContextProvider<iOneDbContext> dbContextProvider)
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

    public async Task<bool> HasReasonsAsync(Guid reasonGroupId)
    {
        var dbContext = await GetDbContextAsync();
        return await dbContext.Set<ResReason>()
            .AnyAsync(x => x.GroupId == reasonGroupId);
    }
}
