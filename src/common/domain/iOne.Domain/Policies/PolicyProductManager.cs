using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace iOne.Policies;

public class PolicyProductManager : DomainService
{
    protected IPolicyProductRepository Repository { get; }

    public PolicyProductManager(IPolicyProductRepository repository)
    {
        Repository = repository;
    }

    public virtual async Task CreateAsync(PolicyProduct policyProduct)
    {
        await Repository.InsertAsync(policyProduct);
    }

    public virtual async Task UpdateAsync(
        PolicyProduct policyProduct,
        string? insurerProductCode = null,
        decimal? amountLiability = null,
        decimal? premiumTotal = null,
        decimal? premium = null,
        decimal? vat = null,
        decimal? discount = null,
        decimal? discountRate = null,
        decimal? markup = null)
    {
        if (insurerProductCode != null)
        {
            policyProduct.UpdateInsurerProductCode(insurerProductCode);
        }
        
        if (amountLiability.HasValue)
        {
            policyProduct.UpdateAmountLiability(amountLiability);
        }
        
        if (premiumTotal.HasValue)
        {
            policyProduct.UpdatePremiumTotal(premiumTotal.Value);
        }
        
        if (premium.HasValue)
        {
            policyProduct.UpdatePremium(premium.Value);
        }
        
        if (vat.HasValue)
        {
            policyProduct.UpdateVat(vat.Value);
        }
        
        if (discount.HasValue)
        {
            policyProduct.UpdateDiscount(discount);
        }
        
        if (discountRate.HasValue)
        {
            policyProduct.UpdateDiscountRate(discountRate);
        }

        if (markup.HasValue)
        {
            policyProduct.UpdateMarkup(markup);
        }
        
        await Repository.UpdateAsync(policyProduct);
    }
}
