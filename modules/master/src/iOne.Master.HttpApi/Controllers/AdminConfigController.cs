using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;
using iOne.Master;
using iOne.Master.Permissions;
using iOne.Master.AdminConfigs;

namespace iOne.Master.Controllers;

[RemoteService(Name = MasterRemoteServiceConsts.RemoteServiceName)]
[Area(MasterRemoteServiceConsts.ModuleName)]
[Route("api/master/admin-configs")]
[Authorize]
public class AdminConfigController : AbpControllerBase
{
    protected IAdminConfigAppService AppService { get; }

    public AdminConfigController(IAdminConfigAppService appService)
    {
        AppService = appService;
    }

    [HttpGet]
    [Authorize(AdminConfigPermissions.View)]
    public virtual Task<PagedResultDto<AdminConfigDto>> GetListAsync(GetAdminConfigsInput input)
    {
        return AppService.GetListAsync(input);
    }

    [HttpGet("{id}")]
    [Authorize(AdminConfigPermissions.View)]
    public virtual Task<AdminConfigDto> GetAsync(Guid id)
    {
        return AppService.GetAsync(id);
    }

    [HttpPost]
    [Authorize(AdminConfigPermissions.Create)]
    public virtual Task<AdminConfigDto> CreateAsync(CreateAdminConfigDto input)
    {
        return AppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    [Authorize(AdminConfigPermissions.Edit)]
    public virtual Task<AdminConfigDto> UpdateAsync(Guid id, UpdateAdminConfigDto input)
    {
        return AppService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    [Authorize(AdminConfigPermissions.Delete)]
    public virtual Task DeleteAsync(Guid id)
    {
        return AppService.DeleteAsync(id);
    }

    [HttpGet("select-list")]
    [Authorize]
    public virtual Task<List<AdminConfigSelectDto>> GetSelectListAsync([FromQuery] string? code = null)
    {
        return AppService.GetSelectListAsync(code);
    }
}

