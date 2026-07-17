using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;
using iOne.Master;
using iOne.Master.Permissions;
using iOne.Master.ResBusinessAuthorities;

namespace iOne.Master.Controllers;

[RemoteService(Name = MasterRemoteServiceConsts.RemoteServiceName)]
[Area(MasterRemoteServiceConsts.ModuleName)]
[Route("api/master/res-business-authorities")]
[Authorize]
public class ResBusinessAuthorityController : AbpControllerBase
{
    protected IResBusinessAuthorityAppService AppService { get; }

    public ResBusinessAuthorityController(IResBusinessAuthorityAppService appService)
    {
        AppService = appService;
    }

    [HttpGet]
    [Authorize(ResBusinessAuthorityPermissions.View)]
    public virtual Task<PagedResultDto<ResBusinessAuthorityDto>> GetListAsync(GetResBusinessAuthoritiesInput input)
    {
        return AppService.GetListAsync(input);
    }

    [HttpGet("{id}")]
    [Authorize(ResBusinessAuthorityPermissions.View)]
    public virtual Task<ResBusinessAuthorityDto> GetAsync(Guid id)
    {
        return AppService.GetAsync(id);
    }

    [HttpPost]
    [Authorize(ResBusinessAuthorityPermissions.Create)]
    public virtual Task<ResBusinessAuthorityDto> CreateAsync(CreateResBusinessAuthorityDto input)
    {
        return AppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    [Authorize(ResBusinessAuthorityPermissions.Edit)]
    public virtual Task<ResBusinessAuthorityDto> UpdateAsync(Guid id, UpdateResBusinessAuthorityDto input)
    {
        return AppService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    [Authorize(ResBusinessAuthorityPermissions.Delete)]
    public virtual Task DeleteAsync(Guid id)
    {
        return AppService.DeleteAsync(id);
    }
}
