using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;
using iOne.Master;
using iOne.Master.Permissions;
using iOne.Master.ResUoms;

namespace iOne.Master.Controllers;

[RemoteService(Name = MasterRemoteServiceConsts.RemoteServiceName)]
[Area(MasterRemoteServiceConsts.ModuleName)]
[Route("api/master/uoms")]
//[Authorize]
public class ResUomController : AbpControllerBase
{
    protected IResUomAppService AppService { get; }

    public ResUomController(IResUomAppService appService)
    {
        AppService = appService;
    }

    [HttpGet]
    [Authorize(ResUomPermissions.View)]
    public virtual Task<PagedResultDto<ResUomDto>> GetListAsync(GetResUomsInput input)
    {
        return AppService.GetListAsync(input);
    }

    [HttpGet("{id}")]
    [Authorize(ResUomPermissions.View)]
    public virtual Task<ResUomDto> GetAsync(Guid id)
    {
        return AppService.GetAsync(id);
    }

    [HttpPost]
    [Authorize(ResUomPermissions.Create)]
    public virtual Task<ResUomDto> CreateAsync(CreateResUomDto input)
    {
        return AppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    [Authorize(ResUomPermissions.Edit)]
    public virtual Task<ResUomDto> UpdateAsync(Guid id, UpdateResUomDto input)
    {
        return AppService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    [Authorize(ResUomPermissions.Delete)]
    public virtual Task DeleteAsync(Guid id)
    {
        return AppService.DeleteAsync(id);
    }
}
