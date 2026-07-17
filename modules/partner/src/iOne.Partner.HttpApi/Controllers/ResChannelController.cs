using System;
using System.Threading.Tasks;
using iOne.Partner;
using iOne.Partner.Permissions;
using iOne.Partner.ResChannels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;

namespace iOne.Partner.Controllers;

[RemoteService(Name = PartnerRemoteServiceConsts.RemoteServiceName)]
[Area(PartnerRemoteServiceConsts.ModuleName)]
[Route("api/partner/channels")]
[Authorize]
public class ResChannelController : AbpControllerBase
{
    protected IResChannelAppService AppService { get; }

    public ResChannelController(IResChannelAppService appService)
    {
        AppService = appService;
    }

    [HttpGet]
    [Authorize(ResChannelPermissions.View)]
    public virtual Task<PagedResultDto<ResChannelDto>> GetListAsync(GetResChannelsInput input)
    {
        return AppService.GetListAsync(input);
    }

    [HttpGet("{id}")]
    [Authorize(ResChannelPermissions.View)]
    public virtual Task<ResChannelDto> GetAsync(Guid id)
    {
        return AppService.GetAsync(id);
    }

    [HttpPost]
    [Authorize(ResChannelPermissions.Create)]
    public virtual Task<ResChannelDto> CreateAsync(CreateResChannelDto input)
    {
        return AppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    [Authorize(ResChannelPermissions.Edit)]
    public virtual Task<ResChannelDto> UpdateAsync(Guid id, UpdateResChannelDto input)
    {
        return AppService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    [Authorize(ResChannelPermissions.Delete)]
    public virtual Task DeleteAsync(Guid id)
    {
        return AppService.DeleteAsync(id);
    }
}

