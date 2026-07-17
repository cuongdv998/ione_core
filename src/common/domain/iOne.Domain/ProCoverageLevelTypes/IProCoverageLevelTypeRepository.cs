using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace iOne.ProCoverageLevelTypes;

public interface IProCoverageLevelTypeRepository : IRepository<ProCoverageLevelType, Guid>
{
    Task<bool> IsCodeExistsAsync(string code, Guid? excludeId = null);
}
