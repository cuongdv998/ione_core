using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace iOne.ResTaskCategories;

public interface IResTaskCategoryRepository : IRepository<ResTaskCategory, Guid>
{
    Task<bool> IsCodeExistsAsync(string code, Guid? excludeId = null);
}
