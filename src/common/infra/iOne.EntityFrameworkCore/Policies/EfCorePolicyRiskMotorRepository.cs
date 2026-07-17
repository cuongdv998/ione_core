using System;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using iOne.EntityFrameworkCore;
using iOne.Policies;

namespace iOne.EntityFrameworkCore.Policies;

public class EfCorePolicyRiskMotorRepository : EfCoreRepository<iOneDbContext, PolicyRiskMotor, Guid>, IPolicyRiskMotorRepository
{
    public EfCorePolicyRiskMotorRepository(IDbContextProvider<iOneDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }
}
