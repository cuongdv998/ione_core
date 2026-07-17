using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace iOne.Hr.HrEmployeeRoles;

public interface IHrEmployeeRoleAppService : ICrudAppService<
    HrEmployeeRoleDto,
    Guid,
    GetHrEmployeeRolesInput,
    CreateHrEmployeeRoleDto,
    UpdateHrEmployeeRoleDto>
{
}

