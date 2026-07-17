using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace iOne.ResSequences;

public class ResSequenceManager : DomainService
{
    protected IResSequenceRepository Repository { get; }

    public ResSequenceManager(IResSequenceRepository repository)
    {
        Repository = repository;
    }

    public virtual async Task CreateAsync(ResSequence entity)
    {
        // Validate code uniqueness
        if (await Repository.IsCodeExistsAsync(entity.Code))
        {
            throw new BusinessException("Master:ResSequence:CodeExists")
                .WithData("Code", entity.Code);
        }

        await Repository.InsertAsync(entity);
    }

    public virtual async Task UpdateAsync(
        ResSequence entity,
        string name,
        string? prefix,
        string? suffix,
        ResSequenceType type,
        int? padding,
        long numberNext,
        int numberIncrement,
        ResSequenceUseDateRange useDateRange,
        ResSequenceDateRangeType? dateRangeType,
        ResSequenceStatus status)
    {
        // ⚠️ QUAN TRỌNG: KHÔNG có UpdateCode - Code là immutable

        entity.UpdateName(name);
        entity.UpdatePrefix(prefix);
        entity.UpdateSuffix(suffix);
        entity.UpdateType(type);
        entity.UpdatePadding(padding);
        entity.UpdateNumberNext(numberNext);
        entity.UpdateNumberIncrement(numberIncrement);
        entity.UpdateUseDateRange(useDateRange);
        entity.UpdateDateRangeType(dateRangeType);
        entity.UpdateStatus(status);

        await Repository.UpdateAsync(entity);
    }

    public virtual async Task DeleteAsync(ResSequence entity)
    {
        // ⚠️ QUAN TRỌNG: Soft delete - xóa trước (ABP audit log) → cập nhật status về Deactive sau
        // 1. Delete để trigger ABP audit log (set IsDeleted = true, DeletionTime, DeleterId)
        await Repository.DeleteAsync(entity);
        
        // 2. Cập nhật status về Deactive
        entity.UpdateStatus(ResSequenceStatus.Deactive);
        await Repository.UpdateAsync(entity);
    }
}

