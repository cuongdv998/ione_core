using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;
using iOne.Master.Permissions;
using iOne.Master.ResPaymentMethods;

namespace iOne.Master.Controllers;

[RemoteService(Name = MasterRemoteServiceConsts.RemoteServiceName)]
[Area(MasterRemoteServiceConsts.ModuleName)]
[Route("api/master/payment-methods")]
//[Authorize]
public class ResPaymentMethodController : AbpControllerBase
{
    protected IResPaymentMethodAppService AppService { get; }

    public ResPaymentMethodController(IResPaymentMethodAppService appService)
    {
        AppService = appService;
    }

    [HttpGet]
    [Authorize(ResPaymentMethodPermissions.View)]
    public virtual Task<PagedResultDto<ResPaymentMethodDto>> GetListAsync(GetResPaymentMethodsInput input)
    {
        return AppService.GetListAsync(input);
    }

    [HttpGet("{id}")]
    [Authorize(ResPaymentMethodPermissions.View)]
    public virtual Task<ResPaymentMethodDto> GetAsync(Guid id)
    {
        return AppService.GetAsync(id);
    }

    [HttpPost]
    [Authorize(ResPaymentMethodPermissions.Create)]
    public virtual Task<ResPaymentMethodDto> CreateAsync(CreateResPaymentMethodDto input)
    {
        return AppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    [Authorize(ResPaymentMethodPermissions.Edit)]
    public virtual Task<ResPaymentMethodDto> UpdateAsync(Guid id, UpdateResPaymentMethodDto input)
    {
        return AppService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    [Authorize(ResPaymentMethodPermissions.Delete)]
    public virtual Task DeleteAsync(Guid id)
    {
        return AppService.DeleteAsync(id);
    }

    [HttpGet("select-list")]
    [Authorize]
    public virtual Task<List<ResPaymentMethodSelectDto>> GetSelectListAsync()
    {
        return AppService.GetSelectListAsync();
    }
}
