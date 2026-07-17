using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace iOne.ResIncidentLevels;

public interface IResIncidentLevelRepository : IRepository<ResIncidentLevel, Guid>
{
    Task<bool> IsCodeExistsAsync(string code, Guid? excludeId = null);
}
