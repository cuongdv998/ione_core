using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;
using iOne.Master;
using iOne.Master.Permissions;
using iOne.Master.ResUserDevices;

namespace iOne.Master.Controllers;

[RemoteService(Name = MasterRemoteServiceConsts.RemoteServiceName)]
[Area(MasterRemoteServiceConsts.ModuleName)]
[Route("api/master/user-devices")]
[Authorize]
public class ResUserDeviceController : AbpControllerBase
{
    protected IResUserDeviceAppService AppService { get; }

    public ResUserDeviceController(IResUserDeviceAppService appService)
    {
        AppService = appService;
    }

    [HttpGet]
    [Authorize(ResUserDevicePermissions.View)]
    public virtual Task<PagedResultDto<ResUserDeviceDto>> GetListAsync(GetResUserDevicesInput input)
    {
        return AppService.GetListAsync(input);
    }

    [HttpGet("{id}")]
    [Authorize(ResUserDevicePermissions.View)]
    public virtual Task<ResUserDeviceDto> GetAsync(Guid id)
    {
        return AppService.GetAsync(id);
    }

    [HttpPost]
    [Authorize(ResUserDevicePermissions.Create)]
    public virtual Task<ResUserDeviceDto> CreateAsync(CreateResUserDeviceDto input)
    {
        return AppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    [Authorize(ResUserDevicePermissions.Edit)]
    public virtual Task<ResUserDeviceDto> UpdateAsync(Guid id, UpdateResUserDeviceDto input)
    {
        return AppService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    [Authorize(ResUserDevicePermissions.Delete)]
    public virtual Task DeleteAsync(Guid id)
    {
        return AppService.DeleteAsync(id);
    }
}
