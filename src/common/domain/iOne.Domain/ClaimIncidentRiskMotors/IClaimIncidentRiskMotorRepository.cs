using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace iOne.ClaimIncidentRiskMotors;

public interface IClaimIncidentRiskMotorRepository : IRepository<ClaimIncidentRiskMotor, Guid>
{
}
