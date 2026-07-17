using System;
using Volo.Abp.Domain.Repositories;

namespace iOne.Policies;

public interface IPolicyRiskObjectRepository : IRepository<PolicyRiskObject, Guid>
{
    // No custom methods for now, following basic CRUD pattern
}
