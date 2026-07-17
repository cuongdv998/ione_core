using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace iOne.Policies;

public interface IPolicyRepository : IRepository<Policy, Guid>
{
    Task<bool> IsPolicyNoExistsAsync(string policyNo, Guid? excludeId = null);
}
