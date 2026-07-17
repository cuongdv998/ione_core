using System;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using iOne.EntityFrameworkCore;
using iOne.ResDocuments;

namespace iOne.EntityFrameworkCore.ResDocuments;

public class EfCoreResDocumentRepository : EfCoreRepository<iOneDbContext, ResDocument, Guid>, IResDocumentRepository
{
    public EfCoreResDocumentRepository(IDbContextProvider<iOneDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }
}

