using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace iOne.ProProductPlanDefinitions;

public interface IProProductPlanDefinitionRepository : IRepository<ProProductPlanDefinition, Guid>
{
    Task<bool> IsCodeExistsAsync(string planCode, Guid? excludeId = null);
}
