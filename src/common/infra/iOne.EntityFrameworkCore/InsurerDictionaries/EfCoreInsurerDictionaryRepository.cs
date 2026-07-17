using System;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using iOne.EntityFrameworkCore;
using iOne.InsurerDictionaries;

namespace iOne.EntityFrameworkCore.InsurerDictionaries;

public class EfCoreInsurerDictionaryRepository : EfCoreRepository<iOneDbContext, InsurerDictionary, Guid>, IInsurerDictionaryRepository
{
    public EfCoreInsurerDictionaryRepository(IDbContextProvider<iOneDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }
}
