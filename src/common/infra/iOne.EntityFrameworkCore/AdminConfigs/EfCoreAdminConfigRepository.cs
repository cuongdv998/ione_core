using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using iOne.EntityFrameworkCore;
using iOne.AdminConfigs;

namespace iOne.EntityFrameworkCore.AdminConfigs;

public class EfCoreAdminConfigRepository : EfCoreRepository<iOneDbContext, AdminConfig, Guid>, IAdminConfigRepository
{
    public EfCoreAdminConfigRepository(IDbContextProvider<iOneDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<bool> IsCodeSubCodeExistsAsync(string code, string subCode, Guid? excludeId = null)
    {
        var query = await GetQueryableAsync();
        query = query.Where(x => x.Code == code && x.SubCode == subCode);

        if (excludeId.HasValue)
        {
            query = query.Where(x => x.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<AdminConfig?> FindByCodeSubCodeAsync(string code, string subCode)
    {
        var query = await GetQueryableAsync();
        return await query.FirstOrDefaultAsync(x => x.Code == code && x.SubCode == subCode);
    }
}

