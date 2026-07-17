using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace iOne.ProCoverageLevelTypes;

public class ProCoverageLevelTypeManager : DomainService
{
    protected IProCoverageLevelTypeRepository Repository { get; }

    public ProCoverageLevelTypeManager(IProCoverageLevelTypeRepository repository)
    {
        Repository = repository;
    }

    public virtual async Task CreateAsync(ProCoverageLevelType coverageLevelType)
    {
        // Check code uniqueness
        if (await Repository.IsCodeExistsAsync(coverageLevelType.Code))
        {
            throw new BusinessException("Product:ProCoverageLevelType:CodeExists")
                .WithData("Code", coverageLevelType.Code);
        }

        await Repository.InsertAsync(coverageLevelType);
    }

    public virtual async Task UpdateAsync(ProCoverageLevelType coverageLevelType, string name, string? description, ProCoverageLevelTypeStatus status)
    {
        coverageLevelType.UpdateName(name);
        coverageLevelType.UpdateDescription(description);
        coverageLevelType.UpdateStatus(status);
        await Repository.UpdateAsync(coverageLevelType);
    }
}
