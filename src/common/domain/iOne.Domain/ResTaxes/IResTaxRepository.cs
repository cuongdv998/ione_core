using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace iOne.ResTaxes;

public interface IResTaxRepository : IRepository<ResTax, Guid>
{
    Task<bool> IsCodeExistsAsync(string code, Guid? excludeId = null, CancellationToken cancellationToken = default);

    Task<ResTax?> FindByCodeAsync(string code, CancellationToken cancellationToken = default);
}

