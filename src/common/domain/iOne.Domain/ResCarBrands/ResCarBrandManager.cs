using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace iOne.ResCarBrands;

public class ResCarBrandManager : DomainService
{
    protected IResCarBrandRepository Repository { get; }

    public ResCarBrandManager(IResCarBrandRepository repository)
    {
        Repository = repository;
    }

    public virtual async Task CreateAsync(ResCarBrand carBrand)
    {
        // Check code uniqueness
        if (await Repository.IsCodeExistsAsync(carBrand.Code))
        {
            throw new BusinessException("Master:ResCarBrand:CodeExists")
                .WithData("Code", carBrand.Code);
        }

        await Repository.InsertAsync(carBrand);
    }

    public virtual async Task UpdateAsync(ResCarBrand carBrand, string name, string? description, ResCarBrandStatus status)
    {
        carBrand.UpdateName(name);
        carBrand.UpdateDescription(description);
        carBrand.UpdateStatus(status);
        await Repository.UpdateAsync(carBrand);
    }
}



