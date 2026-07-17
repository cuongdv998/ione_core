using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace iOne.ProLineOfBusinesses;

public interface IProLineOfBusinessRepository : IRepository<ProLineOfBusiness, Guid>
{
    Task<bool> IsCodeExistsAsync(string code, Guid? excludeId = null, CancellationToken cancellationToken = default);

    Task<ProLineOfBusiness?> FindByCodeAsync(string code, CancellationToken cancellationToken = default);

    Task<List<ProLineOfBusiness>> GetChildrenAsync(Guid parentId, CancellationToken cancellationToken = default);
}




