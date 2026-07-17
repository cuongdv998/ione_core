using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;
using iOne.Product.Permissions;
using iOne.Product.ProProductPlanDefinitions;

namespace iOne.Product.Controllers;

[RemoteService(Name = ProductRemoteServiceConsts.RemoteServiceName)]
[Area(ProductRemoteServiceConsts.ModuleName)]
[Route("api/product/pro-product-plan-definitions")]
//[Authorize]
public class ProProductPlanDefinitionController : AbpControllerBase
{
    protected IProProductPlanDefinitionAppService AppService { get; }

    public ProProductPlanDefinitionController(IProProductPlanDefinitionAppService appService)
    {
        AppService = appService;
    }

    [HttpGet]
    [Authorize(ProProductPermissions.View)]
    public virtual Task<PagedResultDto<ProProductPlanDefinitionDto>> GetListAsync(GetProProductPlanDefinitionsInput input)
    {
        return AppService.GetListAsync(input);
    }

    [HttpGet("{id}")]
    [Authorize(ProProductPermissions.View)]
    public virtual Task<ProProductPlanDefinitionDto> GetAsync(Guid id)
    {
        return AppService.GetAsync(id);
    }

    [HttpPost]
    [Authorize(ProProductPermissions.Create)]
    public virtual Task<ProProductPlanDefinitionDto> CreateAsync(CreateProProductPlanDefinitionDto input)
    {
        return AppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    [Authorize(ProProductPermissions.Edit)]
    public virtual Task<ProProductPlanDefinitionDto> UpdateAsync(Guid id, UpdateProProductPlanDefinitionDto input)
    {
        return AppService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    [Authorize(ProProductPermissions.Delete)]
    public virtual Task DeleteAsync(Guid id)
    {
        return AppService.DeleteAsync(id);
    }

    [HttpGet("by-product/{productId}")]
    [Authorize(ProProductPermissions.View)]
    public virtual Task<List<ProProductPlanDefinitionDto>> GetByProductIdAsync(Guid productId)
    {
        return AppService.GetByProductIdAsync(productId);
    }
}
