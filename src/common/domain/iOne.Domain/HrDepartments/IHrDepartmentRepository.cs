using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace iOne.HrDepartments;

public interface IHrDepartmentRepository : IRepository<HrDepartment, Guid>
{
    Task<bool> IsCodeExistsAsync(string code, Guid? excludeId = null);
    Task<bool> AnyByProvinceIdAsync(Guid provinceId);
    Task<bool> AnyByWardIdAsync(Guid wardId);
    Task<List<HrDepartment>> GetTreeAsync();
    Task<List<HrDepartment>> GetByParentIdAsync(Guid? parentId);
    Task<List<HrDepartment>> GetRootDepartmentsAsync();
    Task<List<HrDepartment>> GetAllActiveAsync();
    /// <summary>
    /// Returns the given department id and all descendant department ids (recursive).
    /// </summary>
    Task<List<Guid>> GetSelfAndDescendantIdsAsync(Guid departmentId);
}

