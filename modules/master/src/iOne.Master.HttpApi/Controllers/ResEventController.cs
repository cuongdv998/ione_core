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
[Route("api/master/events")]
[Authorize]
public class ResEventController : AbpControllerBase
{
    protected IResEventAppService AppService { get; }

    public ResEventController(IResEventAppService appService)
    {
        AppService = appService;
    }

    [HttpGet]
    [Authorize(ResEventPermissions.View)]
    public virtual Task<PagedResultDto<ResEventDto>> GetListAsync(GetResEventsInput input)
    {
        return AppService.GetListAsync(input);
    }

    [HttpGet("{id}")]
    [Authorize(ResEventPermissions.View)]
    public virtual Task<ResEventDto> GetAsync(Guid id)
    {
        return AppService.GetAsync(id);
    }

    [HttpPost]
    [Authorize(ResEventPermissions.Create)]
    public virtual Task<ResEventDto> CreateAsync(CreateResEventDto input)
    {
        return AppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    [Authorize(ResEventPermissions.Edit)]
    public virtual Task<ResEventDto> UpdateAsync(Guid id, UpdateResEventDto input)
    {
        return AppService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    [Authorize(ResEventPermissions.Delete)]
    public virtual Task DeleteAsync(Guid id)
    {
        return AppService.DeleteAsync(id);
    }
}

