using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace iOne.ResObjectItemTypes;

public class ResObjectItemTypeManager : DomainService
{
    protected IResObjectItemTypeRepository Repository { get; }

    public ResObjectItemTypeManager(IResObjectItemTypeRepository repository)
    {
        Repository = repository;
    }

    public virtual async Task CreateAsync(ResObjectItemType objectItemType)
    {
        // Check code uniqueness
        if (await Repository.IsCodeExistsAsync(objectItemType.Code))
        {
            throw new BusinessException("Master:ResObjectItemType:CodeExists")
                .WithData("Code", objectItemType.Code);
        }

        await Repository.InsertAsync(objectItemType);
    }

    public virtual async Task UpdateAsync(ResObjectItemType objectItemType, string name, string? description, ResObjectItemTypeStatus status)
    {
        objectItemType.UpdateName(name);
        objectItemType.UpdateDescription(description);
        objectItemType.UpdateStatus(status);
        await Repository.UpdateAsync(objectItemType);
    }
}
