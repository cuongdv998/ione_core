using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace iOne.Product.ProductPricing;

/// <summary>
/// Application service interface for calculating insurance product premiums.
/// </summary>
public interface IProductPriceAppService : IApplicationService
{
    /// <summary>
    /// Calculates premiums for the given product and coverages based on rate table conditions.
    /// </summary>
    /// <param name="productCode">The product code (from URL path).</param>
    /// <param name="input">The calculation request containing attributes and coverages.</param>
    /// <returns>The calculated premiums for each matching coverage.</returns>
    Task<CalculatePremiumResponseDto> CalculatePremiumAsync(string productCode, CalculatePremiumRequestDto input);
}
