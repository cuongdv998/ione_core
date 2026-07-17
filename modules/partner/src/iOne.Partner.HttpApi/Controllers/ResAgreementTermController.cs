using System;
using System.Threading.Tasks;
using iOne.Partner;
using iOne.Partner.Permissions;
using iOne.Partner.ResAgreementTerms;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;

namespace iOne.Partner.Controllers;

[RemoteService(Name = PartnerRemoteServiceConsts.RemoteServiceName)]
[Area(PartnerRemoteServiceConsts.ModuleName)]
[Route("api/partner/agreement-terms")]
[Authorize]
public class ResAgreementTermController : AbpControllerBase
{
    protected IResAgreementTermAppService AppService { get; }

    public ResAgreementTermController(IResAgreementTermAppService appService)
    {
        AppService = appService;
    }

    [HttpGet]
    [Authorize(ResAgreementTermPermissions.View)]
    public virtual Task<PagedResultDto<ResAgreementTermDto>> GetListAsync(GetResAgreementTermsInput input)
    {
        return AppService.GetListAsync(input);
    }

    [HttpGet("{id}")]
    [Authorize(ResAgreementTermPermissions.View)]
    public virtual Task<ResAgreementTermDto> GetAsync(Guid id)
    {
        return AppService.GetAsync(id);
    }

    [HttpPost]
    [Authorize(ResAgreementTermPermissions.Create)]
    public virtual Task<ResAgreementTermDto> CreateAsync(CreateResAgreementTermDto input)
    {
        return AppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    [Authorize(ResAgreementTermPermissions.Edit)]
    public virtual Task<ResAgreementTermDto> UpdateAsync(Guid id, UpdateResAgreementTermDto input)
    {
        return AppService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    [Authorize(ResAgreementTermPermissions.Delete)]
    public virtual Task DeleteAsync(Guid id)
    {
        return AppService.DeleteAsync(id);
    }
}

