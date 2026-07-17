using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace iOne.ResCurrencies;

public class ResCurrencyManager : DomainService
{
    protected IResCurrencyRepository Repository { get; }

    public ResCurrencyManager(IResCurrencyRepository repository)
    {
        Repository = repository;
    }

    public virtual async Task CreateAsync(ResCurrency currency)
    {
        // Check code uniqueness
        if (await Repository.IsCodeExistsAsync(currency.Code))
        {
            throw new BusinessException("Master:ResCurrency:CodeExists")
                .WithData("Code", currency.Code);
        }

        await Repository.InsertAsync(currency);
    }

    public virtual async Task UpdateAsync(ResCurrency currency, string name, string? description, ResCurrencyStatus status)
    {
        currency.UpdateName(name);
        currency.UpdateDescription(description);
        currency.UpdateStatus(status);
        await Repository.UpdateAsync(currency);
    }
}
