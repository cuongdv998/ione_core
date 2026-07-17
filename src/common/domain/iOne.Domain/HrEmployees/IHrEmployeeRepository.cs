using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace iOne.HrEmployees;

public interface IHrEmployeeRepository : IRepository<HrEmployee, Guid>
{
    Task<bool> IsCodeExistsAsync(string code, Guid? excludeId = null);
    Task<bool> AnyByProvinceIdAsync(Guid provinceId);
    Task<bool> AnyByWardIdAsync(Guid wardId);
    Task<List<HrEmployee>> GetByDepartmentIdAsync(Guid departmentId);
    Task<List<HrEmployee>> GetByOrgIdAsync(Guid orgId);

    /// <summary>
    /// Returns employee ids that belong to the given organization and have the given role (by HrEmployeeRole.Code) via an active HrEmployeeRoleRel.
    /// </summary>
    Task<List<Guid>> GetIdsByOrgIdAndRoleCodeAsync(Guid orgId, string roleCode, CancellationToken cancellationToken = default);
}

