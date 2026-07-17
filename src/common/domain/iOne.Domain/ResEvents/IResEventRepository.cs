using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace iOne.ResEvents;

public interface IResEventRepository : IRepository<ResEvent, Guid>
{
    Task<bool> IsCodeExistsAsync(string code, Guid? excludeId = null);
}

