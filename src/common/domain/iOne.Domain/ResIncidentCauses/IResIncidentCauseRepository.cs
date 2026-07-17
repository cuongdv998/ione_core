using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace iOne.ResIncidentCauses;

public interface IResIncidentCauseRepository : IRepository<ResIncidentCause, Guid>
{
    Task<bool> IsCodeExistsAsync(string code, Guid? excludeId = null);
}
