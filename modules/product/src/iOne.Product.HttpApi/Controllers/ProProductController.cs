using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;
using iOne.Product.Permissions;
using iOne.Product.ProProducts;

namespace iOne.Product.Controllers;

[RemoteService(Name = ProductRemoteServiceConsts.RemoteServiceName)]
[Area(ProductRemoteServiceConsts.ModuleName)]
[Route("api/product/pro-products")]
//[Authorize]
public class ProProductController : AbpControllerBase
{
    protected IProProductAppService AppService { get; }

    public ProProductController(IProProductAppService appService)
    {
        AppService = appService;
    }

    [HttpGet]
    [Authorize(ProProductPermissions.View)]
    public virtual Task<PagedResultDto<ProProductDto>> GetListAsync(GetProProductsInput input)
    {
        return AppService.GetListAsync(input);
    }

    [HttpGet("tree-list")]
    [Authorize(ProProductPermissions.View)]
    public virtual Task<PagedResultDto<ProProductListDto>> GetTreeListAsync(GetProProductsInput input)
    {
        return AppService.GetTreeListAsync(input);
    }

    [HttpGet("{id}")]
    [Authorize(ProProductPermissions.View)]
    public virtual Task<ProProductDto> GetAsync(Guid id)
    {
        return AppService.GetAsync(id);
    }

    [HttpPost]
    [Authorize(ProProductPermissions.Create)]
    public virtual Task<ProProductDto> CreateAsync(CreateProProductDto input)
    {
        return AppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    [Authorize(ProProductPermissions.Edit)]
    public virtual Task<ProProductDto> UpdateAsync(Guid id, [FromBody] UpdateProProductDto input)
    {
        return AppService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    [Authorize(ProProductPermissions.Delete)]
    public virtual Task DeleteAsync(Guid id)
    {
        return AppService.DeleteAsync(id);
    }

    [HttpGet("by-lob-partner")]
    [Authorize(ProProductPermissions.View)]
    public virtual Task<List<ProProductDto>> GetByLobIdAndPartnerIdAsync(
        [FromQuery] Guid lobId,
        [FromQuery] Guid partnerId,
        [FromQuery] Guid? channelId,
        [FromQuery] Guid? appChannelId,
        [FromQuery] string? insurerCode = null)
    {
        return AppService.GetByLobIdAndPartnerIdAsync(lobId, partnerId, channelId, appChannelId, true, insurerCode);
    }

    [HttpGet("by-root-product-plan-definition")]
    [Authorize(ProProductPermissions.View)]
    public virtual Task<List<ProProductDto>> GetByRootProductIdAndPlanDefinitionIdAsync([FromQuery] Guid rootProductId, [FromQuery] Guid planDefinitionId)
    {
        return AppService.GetByRootProductIdAndPlanDefinitionIdAsync(rootProductId, planDefinitionId);
    }

    /// <summary>
    /// Load deductible (mức miễn thường) options per product coverage. Call when product(s) are selected to avoid embedding in by-lob-partner.
    /// </summary>
    [HttpPost("deductible-options")]
    [Authorize(ProProductPermissions.View)]
    public virtual Task<List<DeductibleOptionsForCoverageDto>> GetDeductibleOptionsAsync([FromBody] GetDeductibleOptionsInput input)
    {
        return AppService.GetDeductibleOptionsAsync(input);
    }
}
