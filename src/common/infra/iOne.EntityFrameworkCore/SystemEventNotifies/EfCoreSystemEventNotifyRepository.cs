using System;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using iOne.EntityFrameworkCore;
using iOne.SystemEventNotifies;

namespace iOne.EntityFrameworkCore.SystemEventNotifies;

public class EfCoreSystemEventNotifyRepository : EfCoreRepository<iOneDbContext, SystemEventNotify, Guid>, ISystemEventNotifyRepository
{
    public EfCoreSystemEventNotifyRepository(IDbContextProvider<iOneDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }
}
