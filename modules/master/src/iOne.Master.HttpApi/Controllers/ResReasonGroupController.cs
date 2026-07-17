using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;
using iOne.Master.Permissions;
using iOne.Master.ResReasonGroups;

namespace iOne.Master.Controllers;

[RemoteService(Name = MasterRemoteServiceConsts.RemoteServiceName)]
[Area(MasterRemoteServiceConsts.ModuleName)]
[Route("api/master/reason-groups")]
//[Authorize]
public class ResReasonGroupController : AbpControllerBase
{
    protected IResReasonGroupAppService AppService { get; }

    public ResReasonGroupController(IResReasonGroupAppService appService)
    {
        AppService = appService;
    }

    [HttpGet]
    [Authorize(ResReasonGroupPermissions.View)]
    public virtual Task<PagedResultDto<ResReasonGroupDto>> GetListAsync(GetResReasonGroupsInput input)
    {
        return AppService.GetListAsync(input);
    }

    [HttpGet("{id}")]
    [Authorize(ResReasonGroupPermissions.View)]
    public virtual Task<ResReasonGroupDto> GetAsync(Guid id)
    {
        return AppService.GetAsync(id);
    }

    [HttpPost]
    [Authorize(ResReasonGroupPermissions.Create)]
    public virtual Task<ResReasonGroupDto> CreateAsync(CreateResReasonGroupDto input)
    {
        return AppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    [Authorize(ResReasonGroupPermissions.Edit)]
    public virtual Task<ResReasonGroupDto> UpdateAsync(Guid id, UpdateResReasonGroupDto input)
    {
        return AppService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    [Authorize(ResReasonGroupPermissions.Delete)]
    public virtual Task DeleteAsync(Guid id)
    {
        return AppService.DeleteAsync(id);
    }
}
