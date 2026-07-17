using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace iOne.Policies;

public class PolicyCoverageManager : DomainService
{
    protected IPolicyCoverageRepository Repository { get; }

    public PolicyCoverageManager(IPolicyCoverageRepository repository)
    {
        Repository = repository;
    }

    public virtual async Task CreateAsync(PolicyCoverage policyCoverage)
    {
        await Repository.InsertAsync(policyCoverage);
    }

    public virtual async Task UpdateAsync(
        PolicyCoverage policyCoverage,
        Guid? coverageParentId = null,
        string? insurerCoverageCode = null,
        Guid? tableRateLineId = null,
        decimal? amountLiability = null,
        decimal? quantity = null,
        decimal? netRate = null,
        decimal? baseRate = null,
        decimal? flatRate = null,
        decimal? loading = null,
        decimal? premiumRate = null,
        decimal? premiumTotal = null,
        decimal? premium = null,
        decimal? vat = null,
        decimal? discount = null,
        decimal? discountRate = null)
    {
        if (coverageParentId.HasValue)
        {
            policyCoverage.UpdateCoverageParentId(coverageParentId);
        }
        
        if (insurerCoverageCode != null)
        {
            policyCoverage.UpdateInsurerCoverageCode(insurerCoverageCode);
        }
        
        if (tableRateLineId.HasValue)
        {
            policyCoverage.UpdateTableRateLineId(tableRateLineId);
        }
        
        if (amountLiability.HasValue)
        {
            policyCoverage.UpdateAmountLiability(amountLiability);
        }
        
        if (quantity.HasValue)
        {
            policyCoverage.UpdateQuantity(quantity.Value);
        }
        
        if (netRate.HasValue)
        {
            policyCoverage.UpdateNetRate(netRate);
        }
        
        if (baseRate.HasValue)
        {
            policyCoverage.UpdateBaseRate(baseRate);
        }
        
        if (flatRate.HasValue)
        {
            policyCoverage.UpdateFlatRate(flatRate);
        }
        
        if (loading.HasValue)
        {
            policyCoverage.UpdateLoading(loading);
        }
        
        if (premiumRate.HasValue)
        {
            policyCoverage.UpdatePremiumRate(premiumRate.Value);
        }
        
        if (premiumTotal.HasValue)
        {
            policyCoverage.UpdatePremiumTotal(premiumTotal.Value);
        }
        
        if (premium.HasValue)
        {
            policyCoverage.UpdatePremium(premium.Value);
        }
        
        if (vat.HasValue)
        {
            policyCoverage.UpdateVat(vat.Value);
        }
        
        if (discount.HasValue)
        {
            policyCoverage.UpdateDiscount(discount);
        }
        
        if (discountRate.HasValue)
        {
            policyCoverage.UpdateDiscountRate(discountRate);
        }
        
        await Repository.UpdateAsync(policyCoverage);
    }
}
