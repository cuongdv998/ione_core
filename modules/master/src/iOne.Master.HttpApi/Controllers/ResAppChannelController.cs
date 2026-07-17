using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;
using iOne.Master;
using iOne.Master.Permissions;
using iOne.Master.ResAppChannels;

namespace iOne.Master.Controllers;

[RemoteService(Name = MasterRemoteServiceConsts.RemoteServiceName)]
[Area(MasterRemoteServiceConsts.ModuleName)]
[Route("api/master/app-channels")]
//[Authorize]
public class ResAppChannelController : AbpControllerBase
{
    protected IResAppChannelAppService AppService { get; }

    public ResAppChannelController(IResAppChannelAppService appService)
    {
        AppService = appService;
    }

    [HttpGet]
    [Authorize(ResAppChannelPermissions.View)]
    public virtual Task<PagedResultDto<ResAppChannelDto>> GetListAsync(GetResAppChannelsInput input)
    {
        return AppService.GetListAsync(input);
    }

    [HttpGet("{id}")]
    [Authorize(ResAppChannelPermissions.View)]
    public virtual Task<ResAppChannelDto> GetAsync(Guid id)
    {
        return AppService.GetAsync(id);
    }

    [HttpPost]
    [Authorize(ResAppChannelPermissions.Create)]
    public virtual Task<ResAppChannelDto> CreateAsync(CreateResAppChannelDto input)
    {
        return AppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    [Authorize(ResAppChannelPermissions.Edit)]
    public virtual Task<ResAppChannelDto> UpdateAsync(Guid id, UpdateResAppChannelDto input)
    {
        return AppService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    [Authorize(ResAppChannelPermissions.Delete)]
    public virtual Task DeleteAsync(Guid id)
    {
        return AppService.DeleteAsync(id);
    }
}
