using System;
using System.Threading.Tasks;
using iOne.Hr;
using iOne.Hr.HrEmployeeRoles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;

namespace iOne.Hr.Controllers;

[RemoteService(Name = HrRemoteServiceConsts.RemoteServiceName)]
[Area(HrRemoteServiceConsts.ModuleName)]
[Route("api/hr/employee-roles")]
[Authorize]
public class HrEmployeeRoleController : AbpControllerBase
{
    protected IHrEmployeeRoleAppService AppService { get; }

    public HrEmployeeRoleController(IHrEmployeeRoleAppService appService)
    {
        AppService = appService;
    }

    [HttpGet]
    public virtual Task<PagedResultDto<HrEmployeeRoleDto>> GetListAsync(GetHrEmployeeRolesInput input)
    {
        return AppService.GetListAsync(input);
    }

    [HttpGet("{id}")]
    public virtual Task<HrEmployeeRoleDto> GetAsync(Guid id)
    {
        return AppService.GetAsync(id);
    }

    [HttpPost]
    public virtual Task<HrEmployeeRoleDto> CreateAsync(CreateHrEmployeeRoleDto input)
    {
        return AppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    public virtual Task<HrEmployeeRoleDto> UpdateAsync(Guid id, UpdateHrEmployeeRoleDto input)
    {
        return AppService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    public virtual Task DeleteAsync(Guid id)
    {
        return AppService.DeleteAsync(id);
    }
}

