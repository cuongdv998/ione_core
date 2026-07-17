using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace iOne.ResReasonGroups;

public class ResReasonGroupManager : DomainService
{
    protected IResReasonGroupRepository Repository { get; }

    public ResReasonGroupManager(IResReasonGroupRepository repository)
    {
        Repository = repository;
    }

    public virtual async Task CreateAsync(ResReasonGroup reasonGroup)
    {
        // Check code uniqueness
        if (await Repository.IsCodeExistsAsync(reasonGroup.Code))
        {
            throw new BusinessException("Master:ResReasonGroup:CodeExists")
                .WithData("Code", reasonGroup.Code);
        }

        await Repository.InsertAsync(reasonGroup);
    }

    public virtual async Task UpdateAsync(ResReasonGroup reasonGroup, string name, string? description, ResReasonGroupStatus status)
    {
        reasonGroup.UpdateName(name);
        reasonGroup.UpdateDescription(description);
        reasonGroup.UpdateStatus(status);
        await Repository.UpdateAsync(reasonGroup);
    }
}
