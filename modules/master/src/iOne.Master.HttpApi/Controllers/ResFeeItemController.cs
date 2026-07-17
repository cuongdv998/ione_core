using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;
using iOne.Master.Permissions;
using iOne.Master.ResFeeItems;

namespace iOne.Master.Controllers;

[RemoteService(Name = MasterRemoteServiceConsts.RemoteServiceName)]
[Area(MasterRemoteServiceConsts.ModuleName)]
[Route("api/master/res-fee-item")]
//[Authorize]
public class ResFeeItemController : AbpControllerBase
{
    protected IResFeeItemAppService AppService { get; }

    public ResFeeItemController(IResFeeItemAppService appService)
    {
        AppService = appService;
    }

    [HttpGet]
    [Authorize(ResFeeItemPermissions.View)]
    public virtual Task<PagedResultDto<ResFeeItemDto>> GetListAsync(GetResFeeItemsInput input)
    {
        return AppService.GetListAsync(input);
    }

    [HttpGet("{id}")]
    [Authorize(ResFeeItemPermissions.View)]
    public virtual Task<ResFeeItemDto> GetAsync(Guid id)
    {
        return AppService.GetAsync(id);
    }

    [HttpPost]
    [Authorize(ResFeeItemPermissions.Create)]
    public virtual Task<ResFeeItemDto> CreateAsync(CreateResFeeItemDto input)
    {
        return AppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    [Authorize(ResFeeItemPermissions.Edit)]
    public virtual Task<ResFeeItemDto> UpdateAsync(Guid id, UpdateResFeeItemDto input)
    {
        return AppService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    [Authorize(ResFeeItemPermissions.Delete)]
    public virtual Task DeleteAsync(Guid id)
    {
        return AppService.DeleteAsync(id);
    }
}
