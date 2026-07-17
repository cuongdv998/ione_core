using System;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using iOne.EntityFrameworkCore;
using iOne.Policies;

namespace iOne.EntityFrameworkCore.Policies;

public class EfCorePolicyCoverageLevelRepository : EfCoreRepository<iOneDbContext, PolicyCoverageLevel, Guid>, IPolicyCoverageLevelRepository
{
    public EfCorePolicyCoverageLevelRepository(IDbContextProvider<iOneDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }
}
