using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;
using iOne.Master.Permissions;
using iOne.Master.ResProvinces;

namespace iOne.Master.Controllers;

[RemoteService(Name = MasterRemoteServiceConsts.RemoteServiceName)]
[Area(MasterRemoteServiceConsts.ModuleName)]
[Route("api/master/provinces")]
[Authorize]
public class ResProvinceController : AbpControllerBase
{
    protected IResProvinceAppService AppService { get; }

    public ResProvinceController(IResProvinceAppService appService)
    {
        AppService = appService;
    }

    [HttpGet]
    [Authorize(ResProvincePermissions.View)]
    public virtual Task<PagedResultDto<ResProvinceDto>> GetListAsync(GetResProvincesInput input)
    {
        return AppService.GetListAsync(input);
    }

    [HttpGet("{id}")]
    [Authorize(ResProvincePermissions.View)]
    public virtual Task<ResProvinceDto> GetAsync(Guid id)
    {
        return AppService.GetAsync(id);
    }

    [HttpPost]
    [Authorize(ResProvincePermissions.Create)]
    public virtual Task<ResProvinceDto> CreateAsync(CreateResProvinceDto input)
    {
        return AppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    [Authorize(ResProvincePermissions.Edit)]
    public virtual Task<ResProvinceDto> UpdateAsync(Guid id, UpdateResProvinceDto input)
    {
        return AppService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    [Authorize(ResProvincePermissions.Delete)]
    public virtual Task DeleteAsync(Guid id)
    {
        return AppService.DeleteAsync(id);
    }

    [HttpGet("select-list")]
    [Authorize]
    public virtual Task<List<ResProvinceSelectDto>> GetSelectListAsync()
    {
        return AppService.GetSelectListAsync();
    }
}

