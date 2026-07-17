using System;
using iOne.Policies;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace iOne.EntityFrameworkCore.Policies;

public class EfCorePolicyDocumentRepository
    : EfCoreRepository<iOneDbContext, PolicyDocument, Guid>, IPolicyDocumentRepository
{
    public EfCorePolicyDocumentRepository(IDbContextProvider<iOneDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }
}
