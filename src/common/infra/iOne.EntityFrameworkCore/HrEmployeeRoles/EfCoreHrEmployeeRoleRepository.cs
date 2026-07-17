using System;
using System.Linq;
using System.Threading.Tasks;
using iOne.EntityFrameworkCore;
using iOne.HrEmployeeRoles;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace iOne.EntityFrameworkCore.HrEmployeeRoles;

public class EfCoreHrEmployeeRoleRepository : EfCoreRepository<iOneDbContext, HrEmployeeRole, Guid>,
    IHrEmployeeRoleRepository
{
    public EfCoreHrEmployeeRoleRepository(IDbContextProvider<iOneDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<bool> IsCodeExistsAsync(string code)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet.AnyAsync(x => x.Code == code);
    }
}

