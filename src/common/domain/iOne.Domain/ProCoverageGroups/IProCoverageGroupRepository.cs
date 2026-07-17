using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace iOne.ProCoverageGroups;

public interface IProCoverageGroupRepository : IRepository<ProCoverageGroup, Guid>
{
    Task<bool> IsCodeExistsAsync(string code, Guid? excludeId = null, CancellationToken cancellationToken = default);

    Task<ProCoverageGroup?> FindByCodeAsync(string code, CancellationToken cancellationToken = default);
}

