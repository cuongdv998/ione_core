using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;
using iOne.Master;
using iOne.Master.Permissions;
using iOne.Master.ResEvents;

namespace iOne.Master.Controllers;

[RemoteService(Name = MasterRemoteServiceConsts.RemoteServiceName)]
[Area(MasterRemoteServiceConsts.ModuleName)]
[Route("api/master/event-notify-templates")]
[Authorize]
public class ResEventNotifyTemplateController : AbpControllerBase
{
    protected IResEventNotifyTemplateAppService AppService { get; }

    public ResEventNotifyTemplateController(IResEventNotifyTemplateAppService appService)
    {
        AppService = appService;
    }

    [HttpGet("by-event/{eventId}")]
    [Authorize(ResEventNotifyTemplatePermissions.View)]
    public virtual Task<PagedResultDto<ResEventNotifyTemplateDto>> GetListByEventIdAsync(Guid eventId)
    {
        return AppService.GetListByEventIdAsync(eventId);
    }

    [HttpGet("{id}")]
    [Authorize(ResEventNotifyTemplatePermissions.View)]
    public virtual Task<ResEventNotifyTemplateDto> GetAsync(Guid id)
    {
        return AppService.GetAsync(id);
    }

    [HttpPost]
    [Authorize(ResEventNotifyTemplatePermissions.Create)]
    public virtual Task<ResEventNotifyTemplateDto> CreateAsync(CreateResEventNotifyTemplateDto input)
    {
        return AppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    [Authorize(ResEventNotifyTemplatePermissions.Edit)]
    public virtual Task<ResEventNotifyTemplateDto> UpdateAsync(Guid id, UpdateResEventNotifyTemplateDto input)
    {
        return AppService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    [Authorize(ResEventNotifyTemplatePermissions.Delete)]
    public virtual Task DeleteAsync(Guid id)
    {
        return AppService.DeleteAsync(id);
    }
}

