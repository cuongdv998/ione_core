using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace iOne.ProProductCategorys;

public interface IProProductCategoryRepository : IRepository<ProProductCategory, Guid>
{
    Task<bool> IsCodeExistsAsync(string code, Guid? excludeId = null, CancellationToken cancellationToken = default);

    Task<ProProductCategory?> FindByCodeAsync(string code, CancellationToken cancellationToken = default);

    Task<List<ProProductCategory>> GetChildrenAsync(Guid parentId, CancellationToken cancellationToken = default);

    Task<bool> HasChildrenAsync(Guid parentId, CancellationToken cancellationToken = default);
}
