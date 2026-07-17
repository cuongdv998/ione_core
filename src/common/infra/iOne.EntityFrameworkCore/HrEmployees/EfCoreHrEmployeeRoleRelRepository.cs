using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using iOne.EntityFrameworkCore;
using iOne.HrEmployees;

namespace iOne.EntityFrameworkCore.HrEmployees;

public class EfCoreHrEmployeeRoleRelRepository : EfCoreRepository<iOneDbContext, HrEmployeeRoleRel, Guid>, IHrEmployeeRoleRelRepository
{
    public EfCoreHrEmployeeRoleRelRepository(IDbContextProvider<iOneDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<List<HrEmployeeRoleRel>> GetByEmployeeIdAsync(Guid employeeId)
    {
        var query = await GetQueryableAsync();
        return await query
            .Include(x => x.Role)
            .Where(x => x.EmployeeId == employeeId)
            .OrderBy(x => x.EffectDate)
            .ToListAsync();
    }

    public async Task<bool> HasOverlappingDatesAsync(
        Guid employeeId,
        Guid roleId,
        DateTime effectDate,
        DateTime? expireDate,
        Guid? excludeId = null)
    {
        var query = await GetQueryableAsync();
        query = query.Where(x => x.EmployeeId == employeeId && x.RoleId == roleId);

        if (excludeId.HasValue)
        {
            query = query.Where(x => x.Id != excludeId.Value);
        }

        // Check overlap: 
        // (newEffectDate <= existingExpireDate || existingExpireDate == null) 
        // AND 
        // (newExpireDate >= existingEffectDate || newExpireDate == null)
        var overlappingRecords = await query
            .Where(x =>
                (effectDate <= (x.ExpireDate ?? DateTime.MaxValue)) &&
                ((expireDate ?? DateTime.MaxValue) >= x.EffectDate))
            .AnyAsync();

        return overlappingRecords;
    }
}

