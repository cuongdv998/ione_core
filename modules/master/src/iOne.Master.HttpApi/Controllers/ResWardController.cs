using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;
using iOne.Master.Permissions;
using iOne.Master.ResWards;

namespace iOne.Master.Controllers;

[RemoteService(Name = MasterRemoteServiceConsts.RemoteServiceName)]
[Area(MasterRemoteServiceConsts.ModuleName)]
[Route("api/master/wards")]
[Authorize]
public class ResWardController : AbpControllerBase
{
    protected IResWardAppService AppService { get; }

    public ResWardController(IResWardAppService appService)
    {
        AppService = appService;
    }

    [HttpGet]
    [Authorize(ResWardPermissions.View)]
    public virtual Task<PagedResultDto<ResWardDto>> GetListAsync(GetResWardsInput input)
    {
        return AppService.GetListAsync(input);
    }

    [HttpGet("{id}")]
    [Authorize(ResWardPermissions.View)]
    public virtual Task<ResWardDto> GetAsync(Guid id)
    {
        return AppService.GetAsync(id);
    }

    [HttpPost]
    [Authorize(ResWardPermissions.Create)]
    public virtual Task<ResWardDto> CreateAsync(CreateResWardDto input)
    {
        return AppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    [Authorize(ResWardPermissions.Edit)]
    public virtual Task<ResWardDto> UpdateAsync(Guid id, UpdateResWardDto input)
    {
        return AppService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    [Authorize(ResWardPermissions.Delete)]
    public virtual Task DeleteAsync(Guid id)
    {
        return AppService.DeleteAsync(id);
    }

    [HttpGet("select-list")]
    [Authorize]
    public virtual Task<List<ResWardSelectDto>> GetSelectListAsync([FromQuery] Guid? provinceId = null)
    {
        return AppService.GetSelectListAsync(provinceId);
    }
}

