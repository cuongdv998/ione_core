using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using iOne.Partner;
using iOne.Partner.Permissions;
using iOne.Partner.ResPartners;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;

namespace iOne.Partner.Controllers;

[RemoteService(Name = PartnerRemoteServiceConsts.RemoteServiceName)]
[Area(PartnerRemoteServiceConsts.ModuleName)]
[Route("api/partner/res-partners")]
[Authorize]
public class ResPartnerController : AbpControllerBase
{
    protected IResPartnerAppService AppService { get; }

    public ResPartnerController(IResPartnerAppService appService)
    {
        AppService = appService;
    }

    [HttpGet]
    [Authorize(ResPartnerPermissions.View)]
    public virtual Task<PagedResultDto<ResPartnerDto>> GetListAsync(GetResPartnersInput input)
    {
        return AppService.GetListAsync(input);
    }

    [HttpGet("{id}")]
    [Authorize(ResPartnerPermissions.View)]
    public virtual Task<ResPartnerDto> GetAsync(Guid id)
    {
        return AppService.GetAsync(id);
    }

    [HttpPost]
    [Authorize(ResPartnerPermissions.Create)]
    public virtual Task<ResPartnerDto> CreateAsync(CreateResPartnerDto input)
    {
        return AppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    [Authorize(ResPartnerPermissions.Edit)]
    public virtual Task<ResPartnerDto> UpdateAsync(Guid id, UpdateResPartnerDto input)
    {
        return AppService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    [Authorize(ResPartnerPermissions.Delete)]
    public virtual Task DeleteAsync(Guid id)
    {
        return AppService.DeleteAsync(id);
    }

    [HttpGet("select-list")]
    [Authorize]
    public virtual Task<List<ResPartnerSelectDto>> GetSelectListAsync([FromQuery] string? partnerTypeCode = null)
    {
        return AppService.GetSelectListAsync(partnerTypeCode);
    }
}

