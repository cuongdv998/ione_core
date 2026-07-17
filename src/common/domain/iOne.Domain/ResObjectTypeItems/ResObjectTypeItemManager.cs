using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace iOne.ResObjectTypeItems;

public class ResObjectTypeItemManager : DomainService
{
    protected IResObjectTypeItemRepository Repository { get; }

    public ResObjectTypeItemManager(IResObjectTypeItemRepository repository)
    {
        Repository = repository;
    }

    public virtual async Task CreateAsync(ResObjectTypeItem objectTypeItem)
    {
        // Check code uniqueness
        if (await Repository.IsCodeExistsAsync(objectTypeItem.Code))
        {
            throw new BusinessException("Master:ResObjectTypeItem:CodeExists")
                .WithData("Code", objectTypeItem.Code);
        }

        await Repository.InsertAsync(objectTypeItem);
    }

    public virtual async Task UpdateAsync(
        ResObjectTypeItem objectTypeItem, 
        Guid objectTypeId, 
        Guid? objectItemType, 
        string name, 
        Guid uomId, 
        string? description, 
        ResObjectTypeItemStatus status)
    {
        objectTypeItem.UpdateObjectTypeId(objectTypeId);
        objectTypeItem.UpdateObjectItemType(objectItemType);
        objectTypeItem.UpdateName(name);
        objectTypeItem.UpdateUomId(uomId);
        objectTypeItem.UpdateDescription(description);
        objectTypeItem.UpdateStatus(status);
        await Repository.UpdateAsync(objectTypeItem);
    }
}
