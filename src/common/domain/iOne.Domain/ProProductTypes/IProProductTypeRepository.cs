using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace iOne.ProProductTypes;

public interface IProProductTypeRepository : IRepository<ProProductType, Guid>
{
    Task<bool> IsCodeExistsAsync(string code, Guid? excludeId = null, CancellationToken cancellationToken = default);

    Task<ProProductType?> FindByCodeAsync(string code, CancellationToken cancellationToken = default);
}
