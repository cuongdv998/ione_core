using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace iOne.ResObjectTypeItems;

public interface IResObjectTypeItemRepository : IRepository<ResObjectTypeItem, Guid>
{
    Task<bool> IsCodeExistsAsync(string code, Guid? excludeId = null);
}
