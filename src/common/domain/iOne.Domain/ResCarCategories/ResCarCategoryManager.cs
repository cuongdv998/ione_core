using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace iOne.ResCarCategories;

public class ResCarCategoryManager : DomainService
{
    protected IResCarCategoryRepository Repository { get; }

    public ResCarCategoryManager(IResCarCategoryRepository repository)
    {
        Repository = repository;
    }

    public virtual async Task CreateAsync(ResCarCategory carCategory)
    {
        // Check code uniqueness
        if (await Repository.IsCodeExistsAsync(carCategory.Code))
        {
            throw new BusinessException("Master:ResCarCategory:CodeExists")
                .WithData("Code", carCategory.Code);
        }

        await Repository.InsertAsync(carCategory);
    }

    public virtual async Task UpdateAsync(ResCarCategory carCategory, string name, int seatNumber, string? description, ResCarCategoryStatus status)
    {
        carCategory.UpdateName(name);
        carCategory.UpdateSeatNumber(seatNumber);
        carCategory.UpdateDescription(description);
        carCategory.UpdateStatus(status);
        await Repository.UpdateAsync(carCategory);
    }
}

