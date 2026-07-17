using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace iOne.ResPartnerTypes;

public class ResPartnerTypeManager : DomainService
{
    protected IResPartnerTypeRepository Repository { get; }

    public ResPartnerTypeManager(IResPartnerTypeRepository repository)
    {
        Repository = repository;
    }

    public virtual async Task CreateAsync(ResPartnerType entity)
    {
        // Check Code uniqueness
        if (await Repository.IsCodeExistsAsync(entity.Code))
        {
            throw new BusinessException("ResPartnerType:CodeExists")
                .WithData("Code", entity.Code);
        }

        await Repository.InsertAsync(entity);
    }

    public virtual async Task UpdateAsync(ResPartnerType entity, string name, ResPartnerTypeStatus status)
    {
        // ⚠️ QUAN TRỌNG: Không update Code
        entity.UpdateName(name);
        entity.UpdateStatus(status);

        await Repository.UpdateAsync(entity);
    }
}

