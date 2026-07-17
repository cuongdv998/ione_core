using System;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using iOne.EntityFrameworkCore;
using iOne.WorkTasks;

namespace iOne.EntityFrameworkCore.WorkTasks;

public class EfCoreWorkTaskRepository : EfCoreRepository<iOneDbContext, WorkTask, Guid>, IWorkTaskRepository
{
    public EfCoreWorkTaskRepository(IDbContextProvider<iOneDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }
}
