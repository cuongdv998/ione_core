using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;
using iOne.Master.Permissions;
using iOne.Master.ResCarLines;

namespace iOne.Master.Controllers;

[RemoteService(Name = MasterRemoteServiceConsts.RemoteServiceName)]
[Area(MasterRemoteServiceConsts.ModuleName)]
[Route("api/master/car-lines")]
//[Authorize]
public class ResCarLineController : AbpControllerBase
{
    protected IResCarLineAppService AppService { get; }

    public ResCarLineController(IResCarLineAppService appService)
    {
        AppService = appService;
    }

    [HttpGet]
    [Authorize(ResCarLinePermissions.View)]
    public virtual Task<PagedResultDto<ResCarLineDto>> GetListAsync(GetResCarLinesInput input)
    {
        return AppService.GetListAsync(input);
    }

    [HttpGet("{id}")]
    [Authorize(ResCarLinePermissions.View)]
    public virtual Task<ResCarLineDto> GetAsync(Guid id)
    {
        return AppService.GetAsync(id);
    }

    [HttpPost]
    [Authorize(ResCarLinePermissions.Create)]
    public virtual Task<ResCarLineDto> CreateAsync(CreateResCarLineDto input)
    {
        return AppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    [Authorize(ResCarLinePermissions.Edit)]
    public virtual Task<ResCarLineDto> UpdateAsync(Guid id, UpdateResCarLineDto input)
    {
        return AppService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    [Authorize(ResCarLinePermissions.Delete)]
    public virtual Task DeleteAsync(Guid id)
    {
        return AppService.DeleteAsync(id);
    }
}

