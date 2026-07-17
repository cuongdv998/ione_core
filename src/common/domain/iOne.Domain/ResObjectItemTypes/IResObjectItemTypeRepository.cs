using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace iOne.ResObjectItemTypes;

public interface IResObjectItemTypeRepository : IRepository<ResObjectItemType, Guid>
{
    Task<bool> IsCodeExistsAsync(string code, Guid? excludeId = null);
}
