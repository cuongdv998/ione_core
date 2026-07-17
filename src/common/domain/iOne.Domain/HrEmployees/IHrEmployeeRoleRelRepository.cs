using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace iOne.HrEmployees;

public interface IHrEmployeeRoleRelRepository : IRepository<HrEmployeeRoleRel, Guid>
{
    Task<List<HrEmployeeRoleRel>> GetByEmployeeIdAsync(Guid employeeId);
    Task<bool> HasOverlappingDatesAsync(
        Guid employeeId,
        Guid roleId,
        DateTime effectDate,
        DateTime? expireDate,
        Guid? excludeId = null);
}

