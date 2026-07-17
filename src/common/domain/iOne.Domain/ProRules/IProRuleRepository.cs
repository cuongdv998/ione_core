using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace iOne.ProRules;

public interface IProRuleRepository : IRepository<ProRule, Guid>
{
    Task<bool> IsCodeExistsAsync(string code, Guid? excludeId = null);
}
