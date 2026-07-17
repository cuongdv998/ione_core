using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace iOne.ProProductTypes;

public class ProProductTypeManager : DomainService
{
    protected IProProductTypeRepository Repository { get; }

    public ProProductTypeManager(IProProductTypeRepository repository)
    {
        Repository = repository;
    }

    public virtual async Task CreateAsync(ProProductType productType)
    {
        // Check code uniqueness
        if (await Repository.IsCodeExistsAsync(productType.Code))
        {
            throw new BusinessException("ProProductType:CodeExists")
                .WithData("Code", productType.Code);
        }

        await Repository.InsertAsync(productType);
    }

    public virtual async Task UpdateAsync(
        ProProductType productType,
        Guid lobId,
        string name,
        ProProductTypeStatus status,
        string? description = null)
    {
        // ⚠️ QUAN TRỌNG: Không có parameter code - Code không được phép sửa
        productType.UpdateLobId(lobId);
        productType.UpdateName(name);
        productType.UpdateStatus(status);
        productType.UpdateDescription(description);
        await Repository.UpdateAsync(productType);
    }
}
