using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace iOne.ResMotorClasses;

public class ResMotorClassManager : DomainService
{
    protected IResMotorClassRepository Repository { get; }

    public ResMotorClassManager(IResMotorClassRepository repository)
    {
        Repository = repository;
    }

    public virtual async Task CreateAsync(ResMotorClass motorClass)
    {
        // Check code uniqueness
        if (await Repository.IsCodeExistsAsync(motorClass.Code))
        {
            throw new BusinessException("Master:ResMotorClass:CodeExists")
                .WithData("Code", motorClass.Code);
        }

        await Repository.InsertAsync(motorClass);
    }

    public virtual async Task UpdateAsync(ResMotorClass motorClass, string name, string? description, ResMotorClassStatus status)
    {
        motorClass.UpdateName(name);
        motorClass.UpdateDescription(description);
        motorClass.UpdateStatus(status);
        await Repository.UpdateAsync(motorClass);
    }
}

