using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace iOne.ResUomClasses;

public interface IResUomClassRepository : IRepository<ResUomClass, Guid>
{
    Task<bool> IsCodeExistsAsync(string code, Guid? excludeId = null);
}

