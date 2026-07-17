using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace iOne.HrEmployeePositions;

public interface IHrEmployeePositionRepository : IRepository<HrEmployeePosition, Guid>
{
    Task<bool> IsCodeExistsAsync(string code, Guid? excludeId = null, CancellationToken cancellationToken = default);

    Task<HrEmployeePosition?> FindByCodeAsync(string code, CancellationToken cancellationToken = default);
}

