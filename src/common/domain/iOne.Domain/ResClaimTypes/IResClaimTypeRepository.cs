using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace iOne.ResClaimTypes;

public interface IResClaimTypeRepository : IRepository<ResClaimType, Guid>
{
    Task<bool> IsCodeExistsAsync(string code, Guid? excludeId = null);
}
