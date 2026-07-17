using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;

namespace iOne.InsurerDictionaries;

public class InsurerDictionaryManager : DomainService
{
    protected IInsurerDictionaryRepository Repository { get; }

    public InsurerDictionaryManager(IInsurerDictionaryRepository repository)
    {
        Repository = repository;
    }

    public virtual async Task CreateAsync(InsurerDictionary insurerDictionary)
    {
        await Repository.InsertAsync(insurerDictionary);
    }

    /// <summary>
    /// Updates only allowed fields: InsurerCode, ExtraData, Status, EffectDate, ExpireDate.
    /// BusinessName, InsurerId, OwnCode are not updatable.
    /// </summary>
    public virtual async Task UpdateAsync(
        InsurerDictionary insurerDictionary,
        string insurerCode,
        string? extraData,
        InsurerDictionaryStatus status,
        DateTime effectDate,
        DateTime? expireDate)
    {
        insurerDictionary.UpdateInsurerCode(insurerCode);
        insurerDictionary.UpdateExtraData(extraData);
        insurerDictionary.UpdateStatus(status);
        insurerDictionary.UpdateEffectDate(effectDate);
        insurerDictionary.UpdateExpireDate(expireDate);
        await Repository.UpdateAsync(insurerDictionary);
    }
}
