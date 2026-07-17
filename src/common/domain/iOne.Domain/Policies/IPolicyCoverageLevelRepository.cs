using System;
using Volo.Abp.Domain.Repositories;

namespace iOne.Policies;

public interface IPolicyCoverageLevelRepository : IRepository<PolicyCoverageLevel, Guid>
{
    // No custom methods for now, following basic CRUD pattern
}
