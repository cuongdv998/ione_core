using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace iOne.HrEmployeeLevels;

public interface IHrEmployeeLevelRepository : IRepository<HrEmployeeLevel, Guid>
{
    Task<bool> IsCodeExistsAsync(string code, Guid? excludeId = null, CancellationToken cancellationToken = default);

    Task<HrEmployeeLevel?> FindByCodeAsync(string code, CancellationToken cancellationToken = default);
}

