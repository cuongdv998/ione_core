using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace iOne.ResClaimTypes;

public class ResClaimTypeManager : DomainService
{
    protected IResClaimTypeRepository Repository { get; }

    public ResClaimTypeManager(IResClaimTypeRepository repository)
    {
        Repository = repository;
    }

    public virtual async Task CreateAsync(ResClaimType claimType)
    {
        // Check code uniqueness
        if (await Repository.IsCodeExistsAsync(claimType.Code))
        {
            throw new BusinessException("Master:ResClaimType:CodeExists")
                .WithData("Code", claimType.Code);
        }

        await Repository.InsertAsync(claimType);
    }

    public virtual async Task UpdateAsync(ResClaimType claimType, string name, ResClaimTypeStatus status)
    {
        claimType.UpdateName(name);
        claimType.UpdateStatus(status);
        await Repository.UpdateAsync(claimType);
    }
}
