using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;

namespace iOne.ResObjectItemDepreciations;

public class ResObjectItemDepreciationManager : DomainService
{
    protected IResObjectItemDepreciationRepository Repository { get; }

    public ResObjectItemDepreciationManager(IResObjectItemDepreciationRepository repository)
    {
        Repository = repository;
    }

    public virtual async Task CreateAsync(ResObjectItemDepreciation depreciation)
    {
        await Repository.InsertAsync(depreciation);
    }

    public virtual async Task UpdateAsync(
        ResObjectItemDepreciation depreciation,
        Guid objectTypeItemId,
        Guid carGroupId,
        double usedTimeFrom,
        double usedTimeTo,
        double depreciationPercent,
        DateTime effectDate,
        DateTime? expireDate,
        ResObjectItemDepreciationStatus status)
    {
        depreciation.UpdateObjectTypeItemId(objectTypeItemId);
        depreciation.UpdateCarGroupId(carGroupId);
        depreciation.UpdateUsedTimeRange(usedTimeFrom, usedTimeTo);
        depreciation.UpdateDepreciationPercent(depreciationPercent);
        depreciation.UpdateEffectDate(effectDate);
        depreciation.UpdateExpireDate(expireDate);
        depreciation.UpdateStatus(status);
        await Repository.UpdateAsync(depreciation);
    }
}
