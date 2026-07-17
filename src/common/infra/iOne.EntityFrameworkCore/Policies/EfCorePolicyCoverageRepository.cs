using System;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using iOne.EntityFrameworkCore;
using iOne.Policies;

namespace iOne.EntityFrameworkCore.Policies;

public class EfCorePolicyCoverageRepository : EfCoreRepository<iOneDbContext, PolicyCoverage, Guid>, IPolicyCoverageRepository
{
    public EfCorePolicyCoverageRepository(IDbContextProvider<iOneDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }
}
