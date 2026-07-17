using System;
using Volo.Abp.Domain.Repositories;

namespace iOne.Policies;

public interface IPolicyCoverageRepository : IRepository<PolicyCoverage, Guid>
{
    // No custom methods for now, following basic CRUD pattern
}
