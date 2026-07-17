using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;
using iOne.ResCountries;

namespace iOne.ResProvinces;

public class ResProvinceManager : DomainService
{
    protected IResProvinceRepository Repository { get; }
    protected IResCountryRepository CountryRepository { get; }

    public ResProvinceManager(
        IResProvinceRepository repository,
        IResCountryRepository countryRepository)
    {
        Repository = repository;
        CountryRepository = countryRepository;
    }

    public virtual async Task CreateAsync(ResProvince province)
    {
        // Check country exists
        var country = await CountryRepository.FindAsync(province.CountryId);
        if (country == null)
        {
            throw new BusinessException("Master:ResProvince:CountryNotFound")
                .WithData("CountryId", province.CountryId);
        }

        // Check code uniqueness
        if (await Repository.IsCodeExistsAsync(province.Code))
        {
            throw new BusinessException("Master:ResProvince:CodeExists")
                .WithData("Code", province.Code);
        }

        await Repository.InsertAsync(province);
    }

    public virtual async Task UpdateAsync(
        ResProvince province,
        Guid countryId,
        string name,
        ResProvinceStatus status,
        string? description = null)
    {
        // ⚠️ QUAN TRỌNG: Không có parameter code - Code không được phép sửa

        // Check country exists (if changed)
        if (province.CountryId != countryId)
        {
            var country = await CountryRepository.FindAsync(countryId);
            if (country == null)
            {
                throw new BusinessException("Master:ResProvince:CountryNotFound")
                    .WithData("CountryId", countryId);
            }
        }

        province.UpdateCountryId(countryId);
        province.UpdateName(name);
        province.UpdateStatus(status);
        province.UpdateDescription(description);
        await Repository.UpdateAsync(province);
    }
}

