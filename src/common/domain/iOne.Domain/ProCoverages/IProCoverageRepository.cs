using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace iOne.ProCoverages;

public interface IProCoverageRepository : IRepository<ProCoverage, Guid>
{
    Task<bool> IsCodeExistsAsync(string code, Guid? excludeId = null, CancellationToken cancellationToken = default);

    Task<ProCoverage?> FindByCodeAsync(string code, CancellationToken cancellationToken = default);
}
