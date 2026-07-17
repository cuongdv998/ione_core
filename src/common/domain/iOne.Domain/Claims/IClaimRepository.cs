using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace iOne.Claims;

public interface IClaimRepository : IRepository<Claim, Guid>
{
    Task<bool> IsCodeExistsAsync(string code, Guid? excludeId = null);
}
