using System;
using Volo.Abp.Domain.Repositories;

namespace iOne.Policies;

public interface IPolicyRiskMotorRepository : IRepository<PolicyRiskMotor, Guid>
{
    // No custom methods for now, following basic CRUD pattern
}
