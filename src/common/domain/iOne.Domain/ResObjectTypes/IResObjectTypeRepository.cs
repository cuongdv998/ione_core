using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace iOne.ResObjectTypes;

public interface IResObjectTypeRepository : IRepository<ResObjectType, Guid>
{
    Task<bool> IsCodeExistsAsync(string code, Guid? excludeId = null);
}

