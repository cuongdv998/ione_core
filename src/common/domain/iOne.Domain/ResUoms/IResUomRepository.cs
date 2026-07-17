using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace iOne.ResUoms;

public interface IResUomRepository : IRepository<ResUom, Guid>
{
    Task<bool> IsCodeExistsAsync(string code, Guid? excludeId = null);
}
