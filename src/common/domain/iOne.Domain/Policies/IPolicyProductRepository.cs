using System;
using Volo.Abp.Domain.Repositories;

namespace iOne.Policies;

public interface IPolicyProductRepository : IRepository<PolicyProduct, Guid>
{
    // No custom methods for now, following basic CRUD pattern
}
