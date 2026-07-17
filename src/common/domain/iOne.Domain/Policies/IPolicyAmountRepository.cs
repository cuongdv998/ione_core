using System;
using Volo.Abp.Domain.Repositories;

namespace iOne.Policies;

public interface IPolicyAmountRepository : IRepository<PolicyAmount, Guid>
{
}
