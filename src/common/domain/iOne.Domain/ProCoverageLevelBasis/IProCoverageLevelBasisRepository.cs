using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace iOne.ProCoverageLevelBases;

public interface IProCoverageLevelBasisRepository : IRepository<ProCoverageLevelBasis, Guid>
{
    Task<bool> IsCodeExistsAsync(string code, Guid? excludeId = null);
}
