using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace iOne.ResOrganizationTypes;

public class ResOrganizationTypeManager : DomainService
{
    protected IResOrganizationTypeRepository Repository { get; }

    public ResOrganizationTypeManager(IResOrganizationTypeRepository repository)
    {
        Repository = repository;
    }

    public virtual async Task CreateAsync(ResOrganizationType entity)
    {
        // Check Code uniqueness
        if (await Repository.IsCodeExistsAsync(entity.Code))
        {
            throw new BusinessException("ResOrganizationType:CodeExists")
                .WithData("Code", entity.Code);
        }

        await Repository.InsertAsync(entity);
    }

    public virtual async Task UpdateAsync(ResOrganizationType entity, string name, ResOrganizationTypeStatus status, OrganizationTypeType type)
    {
        // ⚠️ QUAN TRỌNG: Không update Code
        entity.UpdateName(name);
        entity.UpdateStatus(status);
        entity.UpdateType(type);

        await Repository.UpdateAsync(entity);
    }
}

