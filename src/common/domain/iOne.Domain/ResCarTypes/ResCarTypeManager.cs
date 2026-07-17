using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace iOne.ResCarTypes;

public class ResCarTypeManager : DomainService
{
    protected IResCarTypeRepository Repository { get; }

    public ResCarTypeManager(IResCarTypeRepository repository)
    {
        Repository = repository;
    }

    public virtual async Task CreateAsync(ResCarType carType)
    {
        // Check code uniqueness
        if (await Repository.IsCodeExistsAsync(carType.Code))
        {
            throw new BusinessException("Master:ResCarType:CodeExists")
                .WithData("Code", carType.Code);
        }

        await Repository.InsertAsync(carType);
    }

    public virtual async Task UpdateAsync(ResCarType carType, string name, string? description, ResCarTypeStatus status)
    {
        carType.UpdateName(name);
        carType.UpdateDescription(description);
        carType.UpdateStatus(status);
        await Repository.UpdateAsync(carType);
    }
}

