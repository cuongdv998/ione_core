using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace iOne.ResFeeItems;

public interface IResFeeItemRepository : IRepository<ResFeeItem, Guid>
{
    Task<bool> IsCodeExistsAsync(string code, Guid? excludeId = null);
    
    Task<bool> IsTaxInUseAsync(Guid taxId);
}
