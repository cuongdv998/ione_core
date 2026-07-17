using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace iOne.ResTaskCategories;

public class ResTaskCategoryManager : DomainService
{
    protected IResTaskCategoryRepository Repository { get; }

    public ResTaskCategoryManager(IResTaskCategoryRepository repository)
    {
        Repository = repository;
    }

    public virtual async Task CreateAsync(ResTaskCategory entity)
    {
        if (await Repository.IsCodeExistsAsync(entity.Code))
        {
            throw new BusinessException("ResTaskCategory:CodeExists")
                .WithData("Code", entity.Code);
        }

        await Repository.InsertAsync(entity);
    }

    public virtual async Task UpdateAsync(
        ResTaskCategory entity,
        string name,
        ResTaskCategoryStatus status)
    {
        entity.UpdateName(name);
        entity.UpdateStatus(status);
        await Repository.UpdateAsync(entity);
    }
}
