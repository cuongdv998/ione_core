using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using iOne.Hr;
using iOne.Hr.HrDepartments;
using iOne.Hr.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;

namespace iOne.Hr.Controllers;

[RemoteService(Name = HrRemoteServiceConsts.RemoteServiceName)]
[Area(HrRemoteServiceConsts.ModuleName)]
[Route("api/hr/departments")]
[Authorize]
public class HrDepartmentController : AbpControllerBase
{
    protected IHrDepartmentAppService AppService { get; }

    public HrDepartmentController(IHrDepartmentAppService appService)
    {
        AppService = appService;
    }

    [HttpGet]
    [Authorize(HrDepartmentPermissions.View)]
    public virtual Task<PagedResultDto<HrDepartmentDto>> GetListAsync(GetHrDepartmentsInput input)
    {
        return AppService.GetListAsync(input);
    }

    [HttpGet("{id}")]
    [Authorize(HrDepartmentPermissions.View)]
    public virtual Task<HrDepartmentDto> GetAsync(Guid id)
    {
        return AppService.GetAsync(id);
    }

    [HttpPost]
    [Authorize(HrDepartmentPermissions.Create)]
    public virtual Task<HrDepartmentDto> CreateAsync(CreateHrDepartmentDto input)
    {
        return AppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    [Authorize(HrDepartmentPermissions.Edit)]
    public virtual Task<HrDepartmentDto> UpdateAsync(Guid id, UpdateHrDepartmentDto input)
    {
        return AppService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    [Authorize(HrDepartmentPermissions.Delete)]
    public virtual Task DeleteAsync(Guid id)
    {
        return AppService.DeleteAsync(id);
    }

    [HttpGet("tree")]
    [Authorize(HrDepartmentPermissions.View)]
    public virtual Task<ListResultDto<HrDepartmentTreeDto>> GetTreeAsync()
    {
        return AppService.GetTreeAsync();
    }

    [HttpGet("by-parent/{parentId?}")]
    [Authorize(HrDepartmentPermissions.View)]
    public virtual Task<ListResultDto<HrDepartmentDto>> GetByParentIdAsync(Guid? parentId)
    {
        return AppService.GetByParentIdAsync(parentId);
    }

    [HttpGet("roots")]
    [Authorize(HrDepartmentPermissions.View)]
    public virtual Task<ListResultDto<HrDepartmentDto>> GetRootDepartmentsAsync()
    {
        return AppService.GetRootDepartmentsAsync();
    }

    [HttpGet("select-list")]
    [Authorize]
    public virtual Task<List<HrDepartmentSelectDto>> GetSelectListAsync([FromQuery] string? scope = null)
    {
        return AppService.GetSelectListAsync(scope);
    }
}

