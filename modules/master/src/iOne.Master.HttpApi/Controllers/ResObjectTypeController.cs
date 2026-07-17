using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;
using iOne.Master;
using iOne.Master.Permissions;
using iOne.Master.ResObjectTypes;

namespace iOne.Master.Controllers;

[RemoteService(Name = MasterRemoteServiceConsts.RemoteServiceName)]
[Area(MasterRemoteServiceConsts.ModuleName)]
[Route("api/master/object-types")]
//[Authorize]
public class ResObjectTypeController : AbpControllerBase
{
    protected IResObjectTypeAppService AppService { get; }

    public ResObjectTypeController(IResObjectTypeAppService appService)
    {
        AppService = appService;
    }

    [HttpGet]
    [Authorize(ResObjectTypePermissions.View)]
    public virtual Task<PagedResultDto<ResObjectTypeDto>> GetListAsync(GetResObjectTypesInput input)
    {
        return AppService.GetListAsync(input);
    }

    [HttpGet("{id}")]
    [Authorize(ResObjectTypePermissions.View)]
    public virtual Task<ResObjectTypeDto> GetAsync(Guid id)
    {
        return AppService.GetAsync(id);
    }

    [HttpPost]
    [Authorize(ResObjectTypePermissions.Create)]
    public virtual Task<ResObjectTypeDto> CreateAsync(CreateResObjectTypeDto input)
    {
        return AppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    [Authorize(ResObjectTypePermissions.Edit)]
    public virtual Task<ResObjectTypeDto> UpdateAsync(Guid id, UpdateResObjectTypeDto input)
    {
        return AppService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    [Authorize(ResObjectTypePermissions.Delete)]
    public virtual Task DeleteAsync(Guid id)
    {
        return AppService.DeleteAsync(id);
    }
}

