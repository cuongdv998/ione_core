using System;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using iOne.EntityFrameworkCore;
using iOne.WorkInstances;

namespace iOne.EntityFrameworkCore.WorkInstances;

public class EfCoreWorkInstanceRepository : EfCoreRepository<iOneDbContext, WorkInstance, Guid>, IWorkInstanceRepository
{
    public EfCoreWorkInstanceRepository(IDbContextProvider<iOneDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }
}
