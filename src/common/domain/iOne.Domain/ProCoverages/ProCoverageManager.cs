using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace iOne.ProCoverages;

public class ProCoverageManager : DomainService
{
    protected IProCoverageRepository Repository { get; }

    public ProCoverageManager(IProCoverageRepository repository)
    {
        Repository = repository;
    }

    public virtual async Task CreateAsync(ProCoverage coverage)
    {
        // Check code uniqueness
        if (await Repository.IsCodeExistsAsync(coverage.Code))
        {
            throw new BusinessException("ProCoverage:CodeExists")
                .WithData("Code", coverage.Code);
        }

        await Repository.InsertAsync(coverage);
    }

    public virtual async Task UpdateAsync(
        ProCoverage coverage,
        Guid lobId,
        Guid coverageGroupId,
        ProCoverageTermType type,
        string shortName,
        string name,
        ProCoverageStatus status,
        Guid? objectTypeId = null,
        Guid? coverageTypeId = null,
        string? description = null)
    {
        // ⚠️ QUAN TRỌNG: Không có parameter code - Code không được phép sửa
        coverage.UpdateLobId(lobId);
        coverage.UpdateCoverageGroupId(coverageGroupId);
        coverage.UpdateType(type);
        coverage.UpdateShortName(shortName);
        coverage.UpdateName(name);
        coverage.UpdateStatus(status);
        coverage.UpdateObjectTypeId(objectTypeId);
        coverage.UpdateCoverageTypeId(coverageTypeId);
        coverage.UpdateDescription(description);
        await Repository.UpdateAsync(coverage);
    }
}
