using System;
using iOne.Policies;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace iOne.EntityFrameworkCore.Policies;

public class EfCorePolicyAmountRepository
    : EfCoreRepository<iOneDbContext, PolicyAmount, Guid>, IPolicyAmountRepository
{
    public EfCorePolicyAmountRepository(IDbContextProvider<iOneDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }
}
