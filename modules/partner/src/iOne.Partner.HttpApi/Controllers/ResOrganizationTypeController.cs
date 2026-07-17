using System;
using System.Threading.Tasks;
using iOne.Partner;
using iOne.Partner.Permissions;
using iOne.Partner.ResOrganizationTypes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;

namespace iOne.Partner.Controllers;

[RemoteService(Name = PartnerRemoteServiceConsts.RemoteServiceName)]
[Area(PartnerRemoteServiceConsts.ModuleName)]
[Route("api/partner/res-organization-types")]
[Authorize]
public class ResOrganizationTypeController : AbpControllerBase
{
    protected IResOrganizationTypeAppService AppService { get; }

    public ResOrganizationTypeController(IResOrganizationTypeAppService appService)
    {
        AppService = appService;
    }

    [HttpGet]
    [Authorize(ResOrganizationTypePermissions.View)]
    public virtual Task<PagedResultDto<ResOrganizationTypeDto>> GetListAsync(GetResOrganizationTypesInput input)
    {
        return AppService.GetListAsync(input);
    }

    [HttpGet("{id}")]
    [Authorize(ResOrganizationTypePermissions.View)]
    public virtual Task<ResOrganizationTypeDto> GetAsync(Guid id)
    {
        return AppService.GetAsync(id);
    }

    [HttpPost]
    [Authorize(ResOrganizationTypePermissions.Create)]
    public virtual Task<ResOrganizationTypeDto> CreateAsync(CreateResOrganizationTypeDto input)
    {
        return AppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    [Authorize(ResOrganizationTypePermissions.Edit)]
    public virtual Task<ResOrganizationTypeDto> UpdateAsync(Guid id, UpdateResOrganizationTypeDto input)
    {
        return AppService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    [Authorize(ResOrganizationTypePermissions.Delete)]
    public virtual Task DeleteAsync(Guid id)
    {
        return AppService.DeleteAsync(id);
    }
}

