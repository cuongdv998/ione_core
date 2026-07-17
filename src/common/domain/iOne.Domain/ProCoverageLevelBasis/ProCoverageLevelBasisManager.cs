using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace iOne.ProCoverageLevelBases;

public class ProCoverageLevelBasisManager : DomainService
{
    protected IProCoverageLevelBasisRepository Repository { get; }

    public ProCoverageLevelBasisManager(IProCoverageLevelBasisRepository repository)
    {
        Repository = repository;
    }

    public virtual async Task CreateAsync(ProCoverageLevelBasis coverageLevelBasis)
    {
        // Check code uniqueness
        if (await Repository.IsCodeExistsAsync(coverageLevelBasis.Code))
        {
            throw new BusinessException("Product:ProCoverageLevelBasis:CodeExists")
                .WithData("Code", coverageLevelBasis.Code);
        }

        await Repository.InsertAsync(coverageLevelBasis);
    }

    public virtual async Task UpdateAsync(ProCoverageLevelBasis coverageLevelBasis, string name, string? description, ProCoverageLevelBasisStatus status)
    {
        coverageLevelBasis.UpdateName(name);
        coverageLevelBasis.UpdateDescription(description);
        coverageLevelBasis.UpdateStatus(status);
        await Repository.UpdateAsync(coverageLevelBasis);
    }
}
