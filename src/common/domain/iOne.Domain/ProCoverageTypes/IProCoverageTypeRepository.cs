using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace iOne.ProCoverageTypes;

public interface IProCoverageTypeRepository : IRepository<ProCoverageType, Guid>
{
    Task<bool> IsCodeExistsAsync(string code, Guid? excludeId = null, CancellationToken cancellationToken = default);

    Task<ProCoverageType?> FindByCodeAsync(string code, CancellationToken cancellationToken = default);
}

