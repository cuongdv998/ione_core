using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace iOne.ResDamageLevels;

public interface IResDamageLevelRepository : IRepository<ResDamageLevel, Guid>
{
    Task<bool> IsCodeExistsAsync(string code, Guid? excludeId = null);
    Task<bool> IsObjectTypeInUseAsync(Guid objectTypeId);
}

