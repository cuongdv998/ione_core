using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace iOne.ProRuleTypes;

public interface IProRuleTypeRepository : IRepository<ProRuleType, Guid>
{
    Task<bool> IsCodeExistsAsync(string code, Guid? excludeId = null);
}
