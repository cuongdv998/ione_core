using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace iOne.Policies;

public class PolicyCoverageLevelManager : DomainService
{
    protected IPolicyCoverageLevelRepository Repository { get; }

    public PolicyCoverageLevelManager(IPolicyCoverageLevelRepository repository)
    {
        Repository = repository;
    }

    public virtual async Task CreateAsync(PolicyCoverageLevel policyCoverageLevel)
    {
        await Repository.InsertAsync(policyCoverageLevel);
    }

    public virtual async Task UpdateAsync(
        PolicyCoverageLevel policyCoverageLevel,
        Guid? coverageLevelTypeId = null,
        Guid? coverageLevelBasisId = null,
        string? conditionScript = null,
        string? computeScript = null,
        string? amountType = null,
        decimal? fromAmount = null,
        decimal? toAmount = null)
    {
        if (coverageLevelTypeId.HasValue)
        {
            policyCoverageLevel.UpdateCoverageLevelTypeId(coverageLevelTypeId.Value);
        }
        
        if (coverageLevelBasisId.HasValue)
        {
            policyCoverageLevel.UpdateCoverageLevelBasisId(coverageLevelBasisId.Value);
        }
        
        if (conditionScript != null)
        {
            policyCoverageLevel.UpdateConditionScript(conditionScript);
        }
        
        if (computeScript != null)
        {
            policyCoverageLevel.UpdateComputeScript(computeScript);
        }
        
        if (amountType != null)
        {
            policyCoverageLevel.UpdateAmountType(amountType);
        }
        
        if (fromAmount.HasValue)
        {
            policyCoverageLevel.UpdateFromAmount(fromAmount.Value);
        }
        
        if (toAmount.HasValue)
        {
            policyCoverageLevel.UpdateToAmount(toAmount.Value);
        }
        
        await Repository.UpdateAsync(policyCoverageLevel);
    }
}
