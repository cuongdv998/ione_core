using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace iOne.ResObjectTypes;

public class ResObjectTypeManager : DomainService
{
    protected IResObjectTypeRepository Repository { get; }

    public ResObjectTypeManager(IResObjectTypeRepository repository)
    {
        Repository = repository;
    }

    public virtual async Task CreateAsync(ResObjectType objectType)
    {
        // Check code uniqueness
        if (await Repository.IsCodeExistsAsync(objectType.Code))
        {
            throw new BusinessException("Master:ResObjectType:CodeExists")
                .WithData("Code", objectType.Code);
        }

        await Repository.InsertAsync(objectType);
    }

    public virtual async Task UpdateAsync(ResObjectType objectType, string name, string? objectGroup, string? description, ResObjectTypeStatus status)
    {
        objectType.UpdateName(name);
        objectType.UpdateObjectGroup(objectGroup);
        objectType.UpdateDescription(description);
        objectType.UpdateStatus(status);
        await Repository.UpdateAsync(objectType);
    }
}

