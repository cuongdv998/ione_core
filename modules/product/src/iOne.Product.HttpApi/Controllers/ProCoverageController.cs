using System;
using System.Threading.Tasks;
using iOne.Product;
using iOne.Product.ProCoverages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;

namespace iOne.Product.Controllers;

[RemoteService(Name = ProductRemoteServiceConsts.RemoteServiceName)]
[Area(ProductRemoteServiceConsts.ModuleName)]
[Route("api/product/coverages")]
[Authorize]
public class ProCoverageController : AbpControllerBase
{
    protected IProCoverageAppService AppService { get; }

    public ProCoverageController(IProCoverageAppService appService)
    {
        AppService = appService;
    }

    [HttpGet]
    public virtual Task<PagedResultDto<ProCoverageDto>> GetListAsync(GetProCoveragesInput input)
    {
        return AppService.GetListAsync(input);
    }

    [HttpGet("{id}")]
    public virtual Task<ProCoverageDto> GetAsync(Guid id)
    {
        return AppService.GetAsync(id);
    }

    [HttpPost]
    public virtual Task<ProCoverageDto> CreateAsync(CreateProCoverageDto input)
    {
        return AppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    public virtual Task<ProCoverageDto> UpdateAsync(Guid id, UpdateProCoverageDto input)
    {
        return AppService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    public virtual Task DeleteAsync(Guid id)
    {
        return AppService.DeleteAsync(id);
    }
}
