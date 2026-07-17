using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;
using iOne.Master.Permissions;
using iOne.Master.ResReasons;

namespace iOne.Master.Controllers;

[RemoteService(Name = MasterRemoteServiceConsts.RemoteServiceName)]
[Area(MasterRemoteServiceConsts.ModuleName)]
[Route("api/master/reasons")]
//[Authorize]
public class ResReasonController : AbpControllerBase
{
    protected IResReasonAppService AppService { get; }

    public ResReasonController(IResReasonAppService appService)
    {
        AppService = appService;
    }

    [HttpGet]
    [Authorize(ResReasonPermissions.View)]
    public virtual Task<PagedResultDto<ResReasonDto>> GetListAsync(GetResReasonsInput input)
    {
        return AppService.GetListAsync(input);
    }

    [HttpGet("{id}")]
    [Authorize(ResReasonPermissions.View)]
    public virtual Task<ResReasonDto> GetAsync(Guid id)
    {
        return AppService.GetAsync(id);
    }

    [HttpPost]
    [Authorize(ResReasonPermissions.Create)]
    public virtual Task<ResReasonDto> CreateAsync(CreateResReasonDto input)
    {
        return AppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    [Authorize(ResReasonPermissions.Edit)]
    public virtual Task<ResReasonDto> UpdateAsync(Guid id, UpdateResReasonDto input)
    {
        return AppService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    [Authorize(ResReasonPermissions.Delete)]
    public virtual Task DeleteAsync(Guid id)
    {
        return AppService.DeleteAsync(id);
    }

    [HttpGet("select-list-by-group-code")]
    [Authorize]
    public virtual Task<List<ResReasonSelectDto>> GetSelectListByGroupCodeAsync([FromQuery] string code)
    {
        return AppService.GetSelectListByGroupCodeAsync(code);
    }
}
