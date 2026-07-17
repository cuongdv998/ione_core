using System;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using iOne.EntityFrameworkCore;
using iOne.ProProducts;

namespace iOne.EntityFrameworkCore.ProProducts;

public class EfCoreProProductCoverageLevelTermRepository : EfCoreRepository<iOneDbContext, ProProductCoverageLevelTerm, Guid>, IProProductCoverageLevelTermRepository
{
    public EfCoreProProductCoverageLevelTermRepository(IDbContextProvider<iOneDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }
}
