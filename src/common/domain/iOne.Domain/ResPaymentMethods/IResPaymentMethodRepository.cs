using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace iOne.ResPaymentMethods;

public interface IResPaymentMethodRepository : IRepository<ResPaymentMethod, Guid>
{
    Task<bool> IsCodeExistsAsync(string code, Guid? excludeId = null);
}
