using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace iOne.ResFeeItems;

public class ResFeeItemManager : DomainService
{
    protected IResFeeItemRepository Repository { get; }

    public ResFeeItemManager(IResFeeItemRepository repository)
    {
        Repository = repository;
    }

    public virtual async Task CreateAsync(ResFeeItem feeItem)
    {
        // Check code uniqueness
        if (await Repository.IsCodeExistsAsync(feeItem.Code))
        {
            throw new BusinessException("Master:ResFeeItem:CodeExists")
                .WithData("Code", feeItem.Code);
        }

        await Repository.InsertAsync(feeItem);
    }

    public virtual async Task UpdateAsync(ResFeeItem feeItem, string name, string? description, ResFeeItemStatus status, Guid? taxId)
    {
        feeItem.UpdateName(name);
        feeItem.UpdateDescription(description);
        feeItem.UpdateStatus(status);
        feeItem.UpdateTaxId(taxId);
        await Repository.UpdateAsync(feeItem);
    }
}
