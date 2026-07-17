using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace iOne.ResReasons;

public interface IResReasonRepository : IRepository<ResReason, Guid>
{
    Task<bool> IsCodeExistsAsync(string code, Guid? excludeId = null);
}
