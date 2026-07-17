using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace iOne.ResCarTypes;

public interface IResCarTypeRepository : IRepository<ResCarType, Guid>
{
    Task<bool> IsCodeExistsAsync(string code, Guid? excludeId = null);
}

