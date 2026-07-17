using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace iOne.PolicyContracts;

public interface IPolicyContractRepository : IRepository<PolicyContract, Guid>
{
    Task<bool> IsCodeExistsAsync(string code, Guid? excludeId = null);
    Task<bool> IsInsurerContractCodeExistsAsync(Guid? insurerId, string insurerContractCode, Guid? excludeId = null);
}
