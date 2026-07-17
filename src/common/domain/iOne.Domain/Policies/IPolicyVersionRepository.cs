using System;
using Volo.Abp.Domain.Repositories;

namespace iOne.Policies;

public interface IPolicyVersionRepository : IRepository<PolicyVersion, Guid>
{
    // No custom methods for now, following basic CRUD pattern
}
