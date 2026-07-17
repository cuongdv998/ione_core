using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace iOne.Hr.HrEmployees;

public interface IHrEmployeeAppService : ICrudAppService<
    HrEmployeeDto,
    Guid,
    GetHrEmployeesInput,
    CreateHrEmployeeDto,
    UpdateHrEmployeeDto>
{
    Task<List<HrEmployeeDto>> GetByDepartmentIdAsync(Guid departmentId);
    Task<List<HrEmployeeDto>> GetListByIdsAsync(List<Guid> ids);
    Task<List<HrEmployeeRoleRelDto>> GetRolesAsync(Guid employeeId);
    Task<HrEmployeeRoleRelDto> AddRoleAsync(Guid employeeId, CreateHrEmployeeRoleRelDto input);
    Task<HrEmployeeRoleRelDto> UpdateRoleAsync(Guid employeeId, Guid roleRelId, UpdateHrEmployeeRoleRelDto input);
    Task RemoveRoleAsync(Guid employeeId, Guid roleRelId);
    Task<List<HrEmployeeSelectDto>> GetSelectListAsync();
    Task<HrEmployeeDto?> GetCurrentAsync();
}

