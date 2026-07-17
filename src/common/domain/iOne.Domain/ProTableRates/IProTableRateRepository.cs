using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace iOne.ProTableRates;

public interface IProTableRateRepository : IRepository<ProTableRate, Guid>
{
    Task<bool> IsCodeExistsAsync(string code, Guid? excludeId = null, CancellationToken cancellationToken = default);

    Task<ProTableRate?> FindByCodeAsync(string code, CancellationToken cancellationToken = default);
}
