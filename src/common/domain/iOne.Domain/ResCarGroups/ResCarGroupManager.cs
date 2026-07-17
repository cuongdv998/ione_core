using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace iOne.ResCarGroups;

public class ResCarGroupManager : DomainService
{
    protected IResCarGroupRepository Repository { get; }

    public ResCarGroupManager(IResCarGroupRepository repository)
    {
        Repository = repository;
    }

    public virtual async Task CreateAsync(ResCarGroup carGroup)
    {
        // Check code uniqueness
        if (await Repository.IsCodeExistsAsync(carGroup.Code))
        {
            throw new BusinessException("Master:ResCarGroup:CodeExists")
                .WithData("Code", carGroup.Code);
        }

        await Repository.InsertAsync(carGroup);
    }

    public virtual async Task UpdateAsync(ResCarGroup carGroup, Guid? carLineId, string name, string? description, ResCarGroupStatus status)
    {
        carGroup.UpdateCarLineId(carLineId);
        carGroup.UpdateName(name);
        carGroup.UpdateDescription(description);
        carGroup.UpdateStatus(status);
        await Repository.UpdateAsync(carGroup);
    }
}
