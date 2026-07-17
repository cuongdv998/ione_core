using System;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using iOne.EntityFrameworkCore;
using iOne.ResBusinessAssignees;

namespace iOne.EntityFrameworkCore.ResBusinessAssignees;

public class EfCoreResBusinessAssigneeRepository : EfCoreRepository<iOneDbContext, ResBusinessAssignee, Guid>, IResBusinessAssigneeRepository
{
    public EfCoreResBusinessAssigneeRepository(IDbContextProvider<iOneDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }
}
