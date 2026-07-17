using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace iOne.ResCountries;

public class ResCountryManager : DomainService
{
    protected IResCountryRepository Repository { get; }

    public ResCountryManager(IResCountryRepository repository)
    {
        Repository = repository;
    }

    public virtual async Task CreateAsync(ResCountry country)
    {
        // Check code uniqueness
        if (await Repository.IsCodeExistsAsync(country.Code))
        {
            throw new BusinessException("Master:ResCountry:CodeExists")
                .WithData("Code", country.Code);
        }

        await Repository.InsertAsync(country);
    }

    public virtual async Task UpdateAsync(ResCountry country, string name, ResCountryStatus status)
    {
        // ⚠️ QUAN TRỌNG: Không có parameter code - Code không được phép sửa
        country.UpdateName(name);
        country.UpdateStatus(status);
        await Repository.UpdateAsync(country);
    }
}

