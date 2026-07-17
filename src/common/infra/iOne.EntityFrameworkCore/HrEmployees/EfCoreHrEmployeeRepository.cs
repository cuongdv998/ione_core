using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using iOne.EntityFrameworkCore;
using iOne.HrEmployees;

namespace iOne.EntityFrameworkCore.HrEmployees;

public class EfCoreHrEmployeeRepository : EfCoreRepository<iOneDbContext, HrEmployee, Guid>, IHrEmployeeRepository
{
    public EfCoreHrEmployeeRepository(IDbContextProvider<iOneDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<bool> IsCodeExistsAsync(string code, Guid? excludeId = null)
    {
        var query = await GetQueryableAsync();
        query = query.Where(x => x.Code == code);

        if (excludeId.HasValue)
        {
            query = query.Where(x => x.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<bool> AnyByProvinceIdAsync(Guid provinceId)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet.AnyAsync(x => !x.IsDeleted && x.ProvinceId == provinceId);
    }

    public async Task<bool> AnyByWardIdAsync(Guid wardId)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet.AnyAsync(x => !x.IsDeleted && x.WardId == wardId);
    }

    public async Task<List<HrEmployee>> GetByDepartmentIdAsync(Guid departmentId)
    {
        var query = await GetQueryableAsync();
        return await query
            .Include(x => x.Position)
            .Include(x => x.Level)
            .Include(x => x.Partner)
            .Include(x => x.Org)
            .Include(x => x.Department)
            .Include(x => x.Manager)
            .Include(x => x.Province)
            .Include(x => x.Ward)
            .Where(x => !x.IsDeleted && x.DepartmentId == departmentId)
            .OrderBy(x => x.Code)
            .ToListAsync();
    }

    public async Task<List<HrEmployee>> GetByOrgIdAsync(Guid orgId)
    {
        var query = await GetQueryableAsync();
        return await query
            .Include(x => x.Position)
            .Include(x => x.Level)
            .Include(x => x.Partner)
            .Include(x => x.Org)
            .Include(x => x.Department)
            .Include(x => x.Manager)
            .Include(x => x.Province)
            .Include(x => x.Ward)
            .Where(x => !x.IsDeleted && x.OrgId == orgId)
            .OrderBy(x => x.Code)
            .ToListAsync();
    }

    public async Task<List<Guid>> GetIdsByOrgIdAndRoleCodeAsync(Guid orgId, string roleCode, CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();
        var now = DateTime.UtcNow;
        return await dbContext.HrEmployees
            .Where(e => !e.IsDeleted && e.OrgId == orgId)
            .Join(dbContext.HrEmployeeRoleRels, e => e.Id, r => r.EmployeeId, (e, r) => new { e, r })
            .Join(dbContext.HrEmployeeRoles, x => x.r.RoleId, role => role.Id, (x, role) => new { x.e.Id, x.r, role })
            .Where(x => x.role.Code == roleCode && x.r.EffectDate <= now && (x.r.ExpireDate == null || x.r.ExpireDate >= now))
            .Select(x => x.Id)
            .Distinct()
            .ToListAsync(cancellationToken);
    }
}

