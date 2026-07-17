using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace iOne.ProTableRateLines;

public class ProTableRateLineManager : DomainService
{
    protected IProTableRateLineRepository Repository { get; }

    public ProTableRateLineManager(IProTableRateLineRepository repository)
    {
        Repository = repository;
    }

    public virtual async Task CreateAsync(ProTableRateLine tableRateLine)
    {
        // Validate TableRateId exists
        // Note: This validation should be done at the application layer by checking the ProTableRate repository
        // The domain manager focuses on business rules, not data existence checks that require repositories

        await Repository.InsertAsync(tableRateLine);
    }

    public virtual async Task UpdateAsync(
        ProTableRateLine tableRateLine,
        string? name,
        string condition,
        DateTime effectDate,
        Guid? coverageId = null,
        Guid? channelId = null,
        Guid? partnerId = null,
        decimal? minimumRate = null,
        decimal? baseRate = null,
        decimal? flatRate = null,
        decimal? maxDiscount = null,
        decimal? loadingRate = null,
        decimal? loading = null,
        DateTime? expireDate = null)
    {
        tableRateLine.UpdateName(name);
        tableRateLine.UpdateCondition(condition);
        tableRateLine.UpdateEffectDate(effectDate);
        tableRateLine.UpdateCoverageId(coverageId);
        tableRateLine.UpdateChannelId(channelId);
        tableRateLine.UpdatePartnerId(partnerId);
        tableRateLine.UpdateMinimumRate(minimumRate);
        tableRateLine.UpdateBaseRate(baseRate);
        tableRateLine.UpdateFlatRate(flatRate);
        tableRateLine.UpdateMaxDiscount(maxDiscount);
        tableRateLine.UpdateLoadingRate(loadingRate);
        tableRateLine.UpdateLoading(loading);
        tableRateLine.UpdateExpireDate(expireDate);
        await Repository.UpdateAsync(tableRateLine);
    }
}
