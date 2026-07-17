using System;
using System.Threading.Tasks;
using iOne.Partner;
using iOne.Partner.Permissions;
using iOne.Partner.ResPartnerTypes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;

namespace iOne.Partner.Controllers;

[RemoteService(Name = PartnerRemoteServiceConsts.RemoteServiceName)]
[Area(PartnerRemoteServiceConsts.ModuleName)]
[Route("api/partner/partner-types")]
[Authorize]
public class ResPartnerTypeController : AbpControllerBase
{
    protected IResPartnerTypeAppService AppService { get; }

    public ResPartnerTypeController(IResPartnerTypeAppService appService)
    {
        AppService = appService;
    }

    [HttpGet]
    [Authorize(ResPartnerTypePermissions.View)]
    public virtual Task<PagedResultDto<ResPartnerTypeDto>> GetListAsync(GetResPartnerTypesInput input)
    {
        return AppService.GetListAsync(input);
    }

    [HttpGet("{id}")]
    [Authorize(ResPartnerTypePermissions.View)]
    public virtual Task<ResPartnerTypeDto> GetAsync(Guid id)
    {
        return AppService.GetAsync(id);
    }

    [HttpPost]
    [Authorize(ResPartnerTypePermissions.Create)]
    public virtual Task<ResPartnerTypeDto> CreateAsync(CreateResPartnerTypeDto input)
    {
        return AppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    [Authorize(ResPartnerTypePermissions.Edit)]
    public virtual Task<ResPartnerTypeDto> UpdateAsync(Guid id, UpdateResPartnerTypeDto input)
    {
        return AppService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    [Authorize(ResPartnerTypePermissions.Delete)]
    public virtual Task DeleteAsync(Guid id)
    {
        return AppService.DeleteAsync(id);
    }
}

