using System;
using System.Threading.Tasks;
using iOne.Hr.HrDepartmentTypes;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace iOne.Hr.HrDepartmentTypes;

public interface IHrDepartmentTypeAppService : ICrudAppService<
    HrDepartmentTypeDto,
    Guid,
    GetHrDepartmentTypesInput,
    CreateHrDepartmentTypeDto,
    UpdateHrDepartmentTypeDto>
{
}

