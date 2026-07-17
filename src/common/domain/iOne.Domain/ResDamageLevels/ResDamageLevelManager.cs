using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace iOne.ResDamageLevels;

public class ResDamageLevelManager : DomainService
{
    protected IResDamageLevelRepository Repository { get; }

    public ResDamageLevelManager(IResDamageLevelRepository repository)
    {
        Repository = repository;
    }

    public virtual async Task CreateAsync(ResDamageLevel damageLevel)
    {
        // Check code uniqueness
        if (await Repository.IsCodeExistsAsync(damageLevel.Code))
        {
            throw new BusinessException("Master:ResDamageLevel:CodeExists")
                .WithData("Code", damageLevel.Code);
        }

        await Repository.InsertAsync(damageLevel);
    }

    public virtual async Task UpdateAsync(ResDamageLevel damageLevel, Guid objectTypeId, string name, string? description, ResDamageLevelStatus status)
    {
        damageLevel.UpdateObjectTypeId(objectTypeId);
        damageLevel.UpdateName(name);
        damageLevel.UpdateDescription(description);
        damageLevel.UpdateStatus(status);
        await Repository.UpdateAsync(damageLevel);
    }
}

