using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace iOne.BusinessFlows;

public interface IBusinessFlowRepository : IRepository<BusinessFlow, Guid>
{
    Task<bool> ExistsOverlapAsync(
        Guid? organizationId,
        Guid? insurerId,
        string businessCode,
        DateTime effectDate,
        DateTime? expireDate,
        Guid? excludeId = null);
}
