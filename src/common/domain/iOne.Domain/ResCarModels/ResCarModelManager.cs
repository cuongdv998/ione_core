using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;
using iOne.ResCarBrands;

namespace iOne.ResCarModels;

public class ResCarModelManager : DomainService
{
    protected IResCarModelRepository Repository { get; }

    public ResCarModelManager(IResCarModelRepository repository)
    {
        Repository = repository;
    }

    public virtual async Task CreateAsync(ResCarModel carModel)
    {
        // Check code uniqueness
        if (await Repository.IsCodeExistsAsync(carModel.Code))
        {
            throw new BusinessException("Master:ResCarModel:CodeExists")
                .WithData("Code", carModel.Code);
        }

        await Repository.InsertAsync(carModel);
    }

    public virtual async Task UpdateAsync(ResCarModel carModel, string name, string? description, ResCarBrandStatus status)
    {
        carModel.UpdateName(name);
        carModel.UpdateDescription(description);
        carModel.UpdateStatus(status);
        await Repository.UpdateAsync(carModel);
    }
}

