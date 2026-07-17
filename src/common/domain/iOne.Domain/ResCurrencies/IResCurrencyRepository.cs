using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace iOne.ResCurrencies;

public interface IResCurrencyRepository : IRepository<ResCurrency, Guid>
{
    Task<bool> IsCodeExistsAsync(string code, Guid? excludeId = null);
}
