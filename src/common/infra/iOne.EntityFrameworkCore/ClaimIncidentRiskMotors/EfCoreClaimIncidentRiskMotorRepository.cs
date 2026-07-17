using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using iOne.EntityFrameworkCore;
using iOne.ClaimIncidentRiskMotors;

namespace iOne.EntityFrameworkCore.ClaimIncidentRiskMotors;

public class EfCoreClaimIncidentRiskMotorRepository : EfCoreRepository<iOneDbContext, ClaimIncidentRiskMotor, Guid>, IClaimIncidentRiskMotorRepository
{
    public EfCoreClaimIncidentRiskMotorRepository(IDbContextProvider<iOneDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }
}
