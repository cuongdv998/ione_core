using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using iOne.Product;
using iOne.Product.ProLineOfBusinesses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;

namespace iOne.Product.Controllers;

[RemoteService(Name = ProductRemoteServiceConsts.RemoteServiceName)]
[Area(ProductRemoteServiceConsts.ModuleName)]
[Route("api/product/line-of-businesses")]
[Authorize]
public class ProLineOfBusinessController : AbpControllerBase
{
    protected IProLineOfBusinessAppService AppService { get; }

    public ProLineOfBusinessController(IProLineOfBusinessAppService appService)
    {
        AppService = appService;
    }

    [HttpGet]
    public virtual Task<PagedResultDto<ProLineOfBusinessDto>> GetListAsync(GetProLineOfBusinessesInput input)
    {
        return AppService.GetListAsync(input);
    }

    [HttpGet("{id}")]
    public virtual Task<ProLineOfBusinessDto> GetAsync(Guid id)
    {
        return AppService.GetAsync(id);
    }

    [HttpPost]
    public virtual Task<ProLineOfBusinessDto> CreateAsync(CreateProLineOfBusinessDto input)
    {
        return AppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    public virtual Task<ProLineOfBusinessDto> UpdateAsync(Guid id, UpdateProLineOfBusinessDto input)
    {
        return AppService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    public virtual Task DeleteAsync(Guid id)
    {
        return AppService.DeleteAsync(id);
    }

    [HttpGet("select-list")]
    public virtual Task<List<ProLineOfBusinessSelectDto>> GetSelectListAsync()
    {
        return AppService.GetSelectListAsync();
    }
}




