using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace iOne.ResReasonGroups;

public interface IResReasonGroupRepository : IRepository<ResReasonGroup, Guid>
{
    Task<bool> IsCodeExistsAsync(string code, Guid? excludeId = null);
    
    /// <summary>
    /// Check if the reason group has any reasons associated with it
    /// </summary>
    Task<bool> HasReasonsAsync(Guid reasonGroupId);
}
