using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace iOne.HrDepartmentTypes;

public interface IHrDepartmentTypeRepository : IRepository<HrDepartmentType, Guid>
{
    Task<bool> IsCodeExistsAsync(string code, Guid? excludeId = null, CancellationToken cancellationToken = default);

    Task<HrDepartmentType?> FindByCodeAsync(string code, CancellationToken cancellationToken = default);
}

