using System;
using System.Threading.Tasks;
using iOne.Product;
using iOne.Product.ProCoverageTypes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;

namespace iOne.Product.Controllers;

[RemoteService(Name = ProductRemoteServiceConsts.RemoteServiceName)]
[Area(ProductRemoteServiceConsts.ModuleName)]
[Route("api/product/coverage-types")]
[Authorize]
public class ProCoverageTypeController : AbpControllerBase
{
    protected IProCoverageTypeAppService AppService { get; }

    public ProCoverageTypeController(IProCoverageTypeAppService appService)
    {
        AppService = appService;
    }

    [HttpGet]
    public virtual Task<PagedResultDto<ProCoverageTypeDto>> GetListAsync(GetProCoverageTypesInput input)
    {
        return AppService.GetListAsync(input);
    }

    [HttpGet("{id}")]
    public virtual Task<ProCoverageTypeDto> GetAsync(Guid id)
    {
        return AppService.GetAsync(id);
    }

    [HttpPost]
    public virtual Task<ProCoverageTypeDto> CreateAsync(CreateProCoverageTypeDto input)
    {
        return AppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    public virtual Task<ProCoverageTypeDto> UpdateAsync(Guid id, UpdateProCoverageTypeDto input)
    {
        return AppService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    public virtual Task DeleteAsync(Guid id)
    {
        return AppService.DeleteAsync(id);
    }
}

