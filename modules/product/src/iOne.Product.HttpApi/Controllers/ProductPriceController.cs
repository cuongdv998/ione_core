using System.Threading.Tasks;
using iOne.Product.ProductPricing;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;

namespace iOne.Product.Controllers;

/// <summary>
///     Controller for calculating insurance product premiums.
/// </summary>
[RemoteService(Name = ProductRemoteServiceConsts.RemoteServiceName)]
[Area(ProductRemoteServiceConsts.ModuleName)]
[Route("api/product/price")]
// [Authorize(ProProductPermissions.View)]
public class ProductPriceController : AbpControllerBase
{
    public ProductPriceController(IProductPriceAppService appService)
    {
        AppService = appService;
    }

    protected IProductPriceAppService AppService { get; }

    /// <summary>
    ///     Calculates premiums for the given product and coverages.
    /// </summary>
    /// <param name="productCode">The product code.</param>
    /// <param name="input">The calculation request.</param>
    /// <returns>The calculated premiums for each matching coverage.</returns>
    [HttpPost("{productCode}")]
    public virtual Task<CalculatePremiumResponseDto> CalculateAsync(
        string productCode,
        [FromBody] CalculatePremiumRequestDto input)
    {
        return AppService.CalculatePremiumAsync(productCode, input);
    }
}