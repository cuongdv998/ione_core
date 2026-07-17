using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace iOne.ResBusinessAuthorities;

public class ResBusinessAuthorityManager : DomainService
{
    protected IResBusinessAuthorityRepository Repository { get; }

    public ResBusinessAuthorityManager(IResBusinessAuthorityRepository repository)
    {
        Repository = repository;
    }

    public virtual async Task CreateAsync(ResBusinessAuthority entity)
    {
        if (await Repository.IsCodeExistsAsync(entity.Code))
        {
            throw new BusinessException("ResBusinessAuthority:CodeExists")
                .WithData("Code", entity.Code);
        }

        await Repository.InsertAsync(entity);
    }

    public virtual async Task UpdateAsync(
        ResBusinessAuthority entity,
        string name,
        ResBusinessAuthorityStatus status)
    {
        entity.UpdateName(name);
        entity.UpdateStatus(status);
        await Repository.UpdateAsync(entity);
    }
}
