using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace iOne.ProCoverageTypes;

public class ProCoverageTypeManager : DomainService
{
    protected IProCoverageTypeRepository Repository { get; }

    public ProCoverageTypeManager(IProCoverageTypeRepository repository)
    {
        Repository = repository;
    }

    public virtual async Task CreateAsync(ProCoverageType coverageType)
    {
        // Check code uniqueness
        if (await Repository.IsCodeExistsAsync(coverageType.Code))
        {
            throw new BusinessException("ProCoverageType:CodeExists")
                .WithData("Code", coverageType.Code);
        }

        await Repository.InsertAsync(coverageType);
    }

    public virtual async Task UpdateAsync(
        ProCoverageType coverageType, 
        string name, 
        ProCoverageTypeStatus status,
        string? description = null)
    {
        // ⚠️ QUAN TRỌNG: Không có parameter code - Code không được phép sửa
        coverageType.UpdateName(name);
        coverageType.UpdateStatus(status);
        coverageType.UpdateDescription(description);
        await Repository.UpdateAsync(coverageType);
    }
}

