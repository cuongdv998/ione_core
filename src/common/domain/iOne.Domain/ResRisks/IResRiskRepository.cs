using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace iOne.ResRisks;

public interface IResRiskRepository : IRepository<ResRisk, Guid>
{
    Task<bool> IsCodeExistsAsync(string code, Guid? excludeId = null);
    
    /// <summary>
    /// Ki?m tra xem ObjectTypeId có ?ang ???c s? d?ng trong b?t k? Risk nào không
    /// </summary>
    Task<bool> IsObjectTypeInUseAsync(Guid objectTypeId);
}

