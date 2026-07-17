using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace iOne.ResEvents;

public interface IResEventNotifyTemplateRepository : IRepository<ResEventNotifyTemplate, Guid>
{
    Task<List<ResEventNotifyTemplate>> GetListByEventIdAsync(Guid eventId);
    
    Task<bool> IsTemplateExistsAsync(Guid eventId, Guid appChannelId, Guid? excludeId = null);
}

