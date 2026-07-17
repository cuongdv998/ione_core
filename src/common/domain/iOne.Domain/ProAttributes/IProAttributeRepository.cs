using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace iOne.ProAttributes;

public interface IProAttributeRepository : IRepository<ProAttribute, Guid>
{
    Task<bool> IsCodeExistsAsync(string code, Guid? excludeId = null, CancellationToken cancellationToken = default);

    Task<ProAttribute?> FindByCodeAsync(string code, CancellationToken cancellationToken = default);
}
