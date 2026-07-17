using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace iOne.ProCoverageGroups;

public class ProCoverageGroupManager : DomainService
{
    protected IProCoverageGroupRepository Repository { get; }

    public ProCoverageGroupManager(IProCoverageGroupRepository repository)
    {
        Repository = repository;
    }

    public virtual async Task CreateAsync(ProCoverageGroup coverageGroup)
    {
        // Check code uniqueness
        if (await Repository.IsCodeExistsAsync(coverageGroup.Code))
        {
            throw new BusinessException("ProCoverageGroup:CodeExists")
                .WithData("Code", coverageGroup.Code);
        }

        await Repository.InsertAsync(coverageGroup);
    }

    public virtual async Task UpdateAsync(
        ProCoverageGroup coverageGroup, 
        string name, 
        ProCoverageGroupStatus status,
        string? description = null)
    {
        // ⚠️ QUAN TRỌNG: Không có parameter code - Code không được phép sửa
        coverageGroup.UpdateName(name);
        coverageGroup.UpdateStatus(status);
        coverageGroup.UpdateDescription(description);
        await Repository.UpdateAsync(coverageGroup);
    }
}

