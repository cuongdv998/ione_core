using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;
using iOne.Master.Permissions;
using iOne.Master.ResPaymentTypes;

namespace iOne.Master.Controllers;

[RemoteService(Name = MasterRemoteServiceConsts.RemoteServiceName)]
[Area(MasterRemoteServiceConsts.ModuleName)]
[Route("api/master/payment-types")]
public class ResPaymentTypeController : AbpControllerBase
{
    protected IResPaymentTypeAppService AppService { get; }

    public ResPaymentTypeController(IResPaymentTypeAppService appService)
    {
        AppService = appService;
    }

    [HttpGet]
    [Authorize(ResPaymentTypePermissions.View)]
    public virtual Task<PagedResultDto<ResPaymentTypeDto>> GetListAsync(GetResPaymentTypesInput input)
    {
        return AppService.GetListAsync(input);
    }

    [HttpGet("{id}")]
    [Authorize(ResPaymentTypePermissions.View)]
    public virtual Task<ResPaymentTypeDto> GetAsync(Guid id)
    {
        return AppService.GetAsync(id);
    }

    [HttpPost]
    [Authorize(ResPaymentTypePermissions.Create)]
    public virtual Task<ResPaymentTypeDto> CreateAsync(CreateResPaymentTypeDto input)
    {
        return AppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    [Authorize(ResPaymentTypePermissions.Edit)]
    public virtual Task<ResPaymentTypeDto> UpdateAsync(Guid id, UpdateResPaymentTypeDto input)
    {
        return AppService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    [Authorize(ResPaymentTypePermissions.Delete)]
    public virtual Task DeleteAsync(Guid id)
    {
        return AppService.DeleteAsync(id);
    }

    [HttpGet("select-list")]
    [Authorize]
    public virtual Task<List<ResPaymentTypeSelectDto>> GetSelectListAsync()
    {
        return AppService.GetSelectListAsync();
    }
}
