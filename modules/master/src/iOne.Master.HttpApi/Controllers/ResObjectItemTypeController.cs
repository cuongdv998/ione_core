using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;
using iOne.Master.Permissions;
using iOne.Master.ResObjectItemTypes;
using Volo.Abp;

namespace iOne.Master.Controllers;

[RemoteService(Name = MasterRemoteServiceConsts.RemoteServiceName)]
[Area(MasterRemoteServiceConsts.ModuleName)]
[Route("api/master/object-item-types")]
//[Authorize]
public class ResObjectItemTypeController : AbpControllerBase
{
    protected IResObjectItemTypeAppService AppService { get; }

    public ResObjectItemTypeController(IResObjectItemTypeAppService appService)
    {
        AppService = appService;
    }

    [HttpGet]
    [Authorize(ResObjectItemTypePermissions.View)]
    public virtual Task<PagedResultDto<ResObjectItemTypeDto>> GetListAsync(GetResObjectItemTypesInput input)
    {
        return AppService.GetListAsync(input);
    }

    [HttpGet("{id}")]
    [Authorize(ResObjectItemTypePermissions.View)]
    public virtual Task<ResObjectItemTypeDto> GetAsync(Guid id)
    {
        return AppService.GetAsync(id);
    }

    [HttpPost]
    [Authorize(ResObjectItemTypePermissions.Create)]
    public virtual Task<ResObjectItemTypeDto> CreateAsync(CreateResObjectItemTypeDto input)
    {
        return AppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    [Authorize(ResObjectItemTypePermissions.Edit)]
    public virtual Task<ResObjectItemTypeDto> UpdateAsync(Guid id, UpdateResObjectItemTypeDto input)
    {
        return AppService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    [Authorize(ResObjectItemTypePermissions.Delete)]
    public virtual Task DeleteAsync(Guid id)
    {
        return AppService.DeleteAsync(id);
    }
}
