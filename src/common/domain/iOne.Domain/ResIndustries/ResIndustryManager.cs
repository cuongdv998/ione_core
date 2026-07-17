using System;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace iOne.ResIndustries;

public class ResIndustryManager : DomainService
{
    protected IResIndustryRepository Repository { get; }

    public ResIndustryManager(IResIndustryRepository repository)
    {
        Repository = repository;
    }

    public virtual async Task CreateAsync(ResIndustry industry)
    {
        // Validate code uniqueness
        if (await Repository.IsCodeExistsAsync(industry.Code))
        {
            throw new BusinessException("Customer:ResIndustry:CodeExists")
                .WithData("Code", industry.Code);
        }

        await Repository.InsertAsync(industry);
    }

    public virtual async Task UpdateAsync(
        ResIndustry industry,
        string name,
        string? description,
        ResIndustryStatus status)
    {
        // ⚠️ QUAN TRỌNG: KHÔNG có UpdateCode - Code là immutable

        industry.UpdateName(name);
        industry.UpdateDescription(description);
        industry.UpdateStatus(status);

        await Repository.UpdateAsync(industry);
    }

    public virtual async Task DeleteAsync(ResIndustry industry)
    {
        // ⚠️ QUAN TRỌNG: Soft delete - xóa trước (ABP audit log) → cập nhật status về Deactive sau
        // 1. Delete để trigger ABP audit log (set IsDeleted = true, DeletionTime, DeleterId)
        await Repository.DeleteAsync(industry);

        // 2. Cập nhật status về Deactive
        industry.UpdateStatus(ResIndustryStatus.Deactive);
        await Repository.UpdateAsync(industry);
    }
}


