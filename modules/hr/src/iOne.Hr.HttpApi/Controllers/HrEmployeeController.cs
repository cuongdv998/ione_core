using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using iOne.Hr;
using iOne.Hr.HrEmployees;
using iOne.Hr.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;

namespace iOne.Hr.Controllers;

[RemoteService(Name = HrRemoteServiceConsts.RemoteServiceName)]
[Area(HrRemoteServiceConsts.ModuleName)]
[Route("api/hr/employees")]
[Authorize]
public class HrEmployeeController : AbpControllerBase
{
    protected IHrEmployeeAppService AppService { get; }

    public HrEmployeeController(IHrEmployeeAppService appService)
    {
        AppService = appService;
    }

    [HttpGet]
    [Authorize(HrEmployeePermissions.View)]
    public virtual Task<PagedResultDto<HrEmployeeDto>> GetListAsync(GetHrEmployeesInput input)
    {
        return AppService.GetListAsync(input);
    }

    [HttpGet("{id}")]
    [Authorize(HrEmployeePermissions.View)]
    public virtual Task<HrEmployeeDto> GetAsync(Guid id)
    {
        return AppService.GetAsync(id);
    }

    [HttpPost]
    [Authorize(HrEmployeePermissions.Create)]
    public virtual Task<HrEmployeeDto> CreateAsync(CreateHrEmployeeDto input)
    {
        return AppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    [Authorize(HrEmployeePermissions.Edit)]
    public virtual Task<HrEmployeeDto> UpdateAsync(Guid id, UpdateHrEmployeeDto input)
    {
        return AppService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    [Authorize(HrEmployeePermissions.Delete)]
    public virtual Task DeleteAsync(Guid id)
    {
        return AppService.DeleteAsync(id);
    }

    [HttpGet("by-department/{departmentId}")]
    [Authorize(HrEmployeePermissions.View)]
    public virtual Task<List<HrEmployeeDto>> GetByDepartmentIdAsync(Guid departmentId)
    {
        return AppService.GetByDepartmentIdAsync(departmentId);
    }

    [HttpGet("current")]
    [Authorize]
    public virtual Task<HrEmployeeDto?> GetCurrentAsync()
    {
        return AppService.GetCurrentAsync();
    }

    [HttpGet("select-list")]
    [Authorize]
    public virtual Task<List<HrEmployeeSelectDto>> GetSelectListAsync()
    {
        return AppService.GetSelectListAsync();
    }

    [HttpGet("{employeeId}/roles")]
    [Authorize(HrEmployeePermissions.View)]
    public virtual Task<List<HrEmployeeRoleRelDto>> GetRolesAsync(Guid employeeId)
    {
        return AppService.GetRolesAsync(employeeId);
    }

    [HttpPost("{employeeId}/roles")]
    [Authorize(HrEmployeePermissions.Edit)]
    public virtual Task<HrEmployeeRoleRelDto> AddRoleAsync(Guid employeeId, CreateHrEmployeeRoleRelDto input)
    {
        return AppService.AddRoleAsync(employeeId, input);
    }

    [HttpPut("{employeeId}/roles/{roleRelId}")]
    [Authorize(HrEmployeePermissions.Edit)]
    public virtual Task<HrEmployeeRoleRelDto> UpdateRoleAsync(Guid employeeId, Guid roleRelId, UpdateHrEmployeeRoleRelDto input)
    {
        return AppService.UpdateRoleAsync(employeeId, roleRelId, input);
    }

    [HttpDelete("{employeeId}/roles/{roleRelId}")]
    [Authorize(HrEmployeePermissions.Edit)]
    public virtual Task RemoveRoleAsync(Guid employeeId, Guid roleRelId)
    {
        return AppService.RemoveRoleAsync(employeeId, roleRelId);
    }
}

