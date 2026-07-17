using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace iOne.ProAttributes;

public class ProAttributeManager : DomainService
{
    protected IProAttributeRepository Repository { get; }

    public ProAttributeManager(IProAttributeRepository repository)
    {
        Repository = repository;
    }

    public virtual async Task CreateAsync(ProAttribute attribute)
    {
        // Check code uniqueness
        if (await Repository.IsCodeExistsAsync(attribute.Code))
        {
            throw new BusinessException("ProAttribute:CodeExists")
                .WithData("Code", attribute.Code);
        }

        await Repository.InsertAsync(attribute);
    }

    public virtual async Task UpdateAsync(
        ProAttribute attribute,
        string name,
        ProAttributeStatus status,
        ProAttributeSpec spec,
        ProAttributeDataType dataType,
        string? description = null,
        string? dataPath = null,
        string? computeScript = null,
        string? clearDataScript = null)
    {
        // ⚠️ QUAN TRỌNG: Không có parameter code - Code không được phép sửa
        attribute.UpdateName(name);
        attribute.UpdateStatus(status);
        attribute.UpdateSpec(spec);
        attribute.UpdateDataType(dataType);
        attribute.UpdateDescription(description);
        attribute.UpdateDataPath(dataPath);
        attribute.UpdateComputeScript(computeScript);
        attribute.UpdateClearDataScript(clearDataScript);
        await Repository.UpdateAsync(attribute);
    }
}
