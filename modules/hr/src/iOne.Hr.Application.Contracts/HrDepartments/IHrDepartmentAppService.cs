using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace iOne.Hr.HrDepartments;

public interface IHrDepartmentAppService : ICrudAppService<
    HrDepartmentDto,
    Guid,
    GetHrDepartmentsInput,
    CreateHrDepartmentDto,
    UpdateHrDepartmentDto>
{
    Task<ListResultDto<HrDepartmentTreeDto>> GetTreeAsync();
    Task<ListResultDto<HrDepartmentDto>> GetByParentIdAsync(Guid? parentId);
    Task<ListResultDto<HrDepartmentDto>> GetRootDepartmentsAsync();
    Task<List<HrDepartmentSelectDto>> GetSelectListAsync(string? scope = null);
}

