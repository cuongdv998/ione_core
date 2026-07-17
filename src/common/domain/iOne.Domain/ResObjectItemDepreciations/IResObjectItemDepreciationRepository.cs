using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace iOne.ResObjectItemDepreciations;

public interface IResObjectItemDepreciationRepository : IRepository<ResObjectItemDepreciation, Guid>
{
    // Add custom repository methods here if needed
    Task<bool> IsCarGroupInUseAsync(Guid carGroupId);
    Task<bool> IsObjectTypeItemInUseAsync(Guid objectTypeItemId);
}
