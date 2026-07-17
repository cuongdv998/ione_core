using System;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using iOne.EntityFrameworkCore;
using iOne.ProProductDistributions;

namespace iOne.EntityFrameworkCore.ProProductDistributions;

public class EfCoreProProductDistributionRepository : EfCoreRepository<iOneDbContext, ProProductDistribution, Guid>, IProProductDistributionRepository
{
    public EfCoreProProductDistributionRepository(IDbContextProvider<iOneDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }
}
