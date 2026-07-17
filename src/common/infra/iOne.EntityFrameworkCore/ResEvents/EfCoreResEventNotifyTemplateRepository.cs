using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using iOne.EntityFrameworkCore;
using iOne.ResEvents;

namespace iOne.EntityFrameworkCore.ResEvents;

public class EfCoreResEventNotifyTemplateRepository : EfCoreRepository<iOneDbContext, ResEventNotifyTemplate, Guid>, IResEventNotifyTemplateRepository
{
    public EfCoreResEventNotifyTemplateRepository(IDbContextProvider<iOneDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<List<ResEventNotifyTemplate>> GetListByEventIdAsync(Guid eventId)
    {
        var query = await GetQueryableAsync();
        return await query
            .Where(x => x.EventId == eventId)
            .Include(x => x.AppChannel)
            .ToListAsync();
    }

    public async Task<bool> IsTemplateExistsAsync(Guid eventId, Guid appChannelId, Guid? excludeId = null)
    {
        var query = await GetQueryableAsync();
        query = query.Where(x => x.EventId == eventId && x.AppChannelId == appChannelId);

        if (excludeId.HasValue)
        {
            query = query.Where(x => x.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }
}

