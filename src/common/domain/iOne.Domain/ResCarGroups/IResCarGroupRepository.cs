using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace iOne.ResCarGroups;

public interface IResCarGroupRepository : IRepository<ResCarGroup, Guid>
{
    Task<bool> IsCodeExistsAsync(string code, Guid? excludeId = null);
    Task<bool> HasGroupsForLineAsync(Guid carLineId);
}
