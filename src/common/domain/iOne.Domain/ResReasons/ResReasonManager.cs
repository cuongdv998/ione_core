using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;
using iOne.ResReasonGroups;

namespace iOne.ResReasons;

public class ResReasonManager : DomainService
{
    protected IResReasonRepository Repository { get; }

    public ResReasonManager(IResReasonRepository repository)
    {
        Repository = repository;
    }

    public virtual async Task CreateAsync(ResReason reason)
    {
        // Check code uniqueness
        if (await Repository.IsCodeExistsAsync(reason.Code))
        {
            throw new BusinessException("Master:ResReason:CodeExists")
                .WithData("Code", reason.Code);
        }

        await Repository.InsertAsync(reason);
    }

    public virtual async Task UpdateAsync(ResReason reason, string name, string? description, ResReasonGroupStatus status)
    {
        reason.UpdateName(name);
        reason.UpdateDescription(description);
        reason.UpdateStatus(status);
        await Repository.UpdateAsync(reason);
    }
}
