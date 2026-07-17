using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;
using iOne.Master;
using iOne.Master.Permissions;
using iOne.Master.ResBusinessAssignees;

namespace iOne.Master.Controllers;

[RemoteService(Name = MasterRemoteServiceConsts.RemoteServiceName)]
[Area(MasterRemoteServiceConsts.ModuleName)]
[Route("api/master/res-business-assignees")]
[Authorize]
public class ResBusinessAssigneeController : AbpControllerBase
{
    protected IResBusinessAssigneeAppService AppService { get; }

    public ResBusinessAssigneeController(IResBusinessAssigneeAppService appService)
    {
        AppService = appService;
    }

    [HttpGet]
    [Authorize(ResBusinessAssigneePermissions.View)]
    public virtual Task<PagedResultDto<ResBusinessAssigneeDto>> GetListAsync(GetResBusinessAssigneesInput input)
    {
        return AppService.GetListAsync(input);
    }

    [HttpGet("{id}")]
    [Authorize(ResBusinessAssigneePermissions.View)]
    public virtual Task<ResBusinessAssigneeDto> GetAsync(Guid id)
    {
        return AppService.GetAsync(id);
    }

    [HttpPost]
    [Authorize(ResBusinessAssigneePermissions.Create)]
    public virtual Task<ResBusinessAssigneeDto> CreateAsync(CreateResBusinessAssigneeDto input)
    {
        return AppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    [Authorize(ResBusinessAssigneePermissions.Edit)]
    public virtual Task<ResBusinessAssigneeDto> UpdateAsync(Guid id, UpdateResBusinessAssigneeDto input)
    {
        return AppService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    [Authorize(ResBusinessAssigneePermissions.Delete)]
    public virtual Task DeleteAsync(Guid id)
    {
        return AppService.DeleteAsync(id);
    }
}
