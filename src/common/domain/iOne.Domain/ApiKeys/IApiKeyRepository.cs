using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace iOne.ApiKeys;

public interface IApiKeyRepository : IRepository<ApiKey, Guid>
{
    Task<ApiKey?> FindByPrefixAsync(string prefix, CancellationToken cancellationToken = default);

    Task<List<ApiKey>> GetListByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
