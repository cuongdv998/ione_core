using System;
using System.Threading.Tasks;
using iOne.Product;
using iOne.Product.ProProductCategorys;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;

namespace iOne.Product.Controllers;

[RemoteService(Name = ProductRemoteServiceConsts.RemoteServiceName)]
[Area(ProductRemoteServiceConsts.ModuleName)]
[Route("api/product/product-categories")]
[Authorize]
public class ProProductCategoryController : AbpControllerBase
{
    protected IProProductCategoryAppService AppService { get; }

    public ProProductCategoryController(IProProductCategoryAppService appService)
    {
        AppService = appService;
    }

    [HttpGet]
    public virtual Task<PagedResultDto<ProProductCategoryDto>> GetListAsync(GetProProductCategorysInput input)
    {
        return AppService.GetListAsync(input);
    }

    [HttpGet("{id}")]
    public virtual Task<ProProductCategoryDto> GetAsync(Guid id)
    {
        return AppService.GetAsync(id);
    }

    [HttpPost]
    public virtual Task<ProProductCategoryDto> CreateAsync(CreateProProductCategoryDto input)
    {
        return AppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    public virtual Task<ProProductCategoryDto> UpdateAsync(Guid id, UpdateProProductCategoryDto input)
    {
        return AppService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    public virtual Task DeleteAsync(Guid id)
    {
        return AppService.DeleteAsync(id);
    }
}
