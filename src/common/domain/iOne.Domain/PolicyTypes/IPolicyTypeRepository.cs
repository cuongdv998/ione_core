using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace iOne.PolicyTypes;

public interface IPolicyTypeRepository : IRepository<PolicyType, Guid>
{
    Task<bool> IsCodeExistsAsync(string code, Guid? excludeId = null);
}
