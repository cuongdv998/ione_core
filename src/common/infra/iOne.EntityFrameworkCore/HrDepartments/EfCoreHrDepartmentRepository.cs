using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using iOne.EntityFrameworkCore;
using iOne.HrDepartments;

namespace iOne.EntityFrameworkCore.HrDepartments;

public class EfCoreHrDepartmentRepository : EfCoreRepository<iOneDbContext, HrDepartment, Guid>, IHrDepartmentRepository
{
    public EfCoreHrDepartmentRepository(IDbContextProvider<iOneDbContext> dbContextProvider)
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

    public async Task<List<HrDepartment>> GetTreeAsync()
    {
        var query = await GetQueryableAsync();
        var allDepartments = await query
            .Include(x => x.Parent)
            .Include(x => x.Org)
            .Include(x => x.Type)
            .Include(x => x.Partner)
            .Include(x => x.Province)
            .Include(x => x.Ward)
            .Include(x => x.Bank)
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.Code)
            .ToListAsync();

        // Clear existing children to avoid duplicates
        // EF Core may have already loaded children through navigation property
        foreach (var department in allDepartments)
        {
            department.Children.Clear();
        }

        // Build tree structure
        var rootDepartments = allDepartments.Where(x => x.ParentId == null).ToList();
        var departmentDict = allDepartments.ToDictionary(x => x.Id);

        foreach (var department in allDepartments)
        {
            if (department.ParentId.HasValue && departmentDict.TryGetValue(department.ParentId.Value, out var parent))
            {
                parent.Children.Add(department);
            }
        }

        return rootDepartments;
    }

    public async Task<List<HrDepartment>> GetByParentIdAsync(Guid? parentId)
    {
        var query = await GetQueryableAsync();
        return await query
            .Include(x => x.Parent)
            .Include(x => x.Org)
            .Include(x => x.Type)
            .Include(x => x.Partner)
            .Include(x => x.Province)
            .Include(x => x.Ward)
            .Include(x => x.Bank)
            .Where(x => !x.IsDeleted && x.ParentId == parentId)
            .OrderBy(x => x.Code)
            .ToListAsync();
    }

    public async Task<List<HrDepartment>> GetRootDepartmentsAsync()
    {
        var query = await GetQueryableAsync();
        return await query
            .Include(x => x.Parent)
            .Include(x => x.Org)
            .Include(x => x.Type)
            .Include(x => x.Partner)
            .Include(x => x.Province)
            .Include(x => x.Ward)
            .Include(x => x.Bank)
            .Where(x => !x.IsDeleted && x.ParentId == null)
            .OrderBy(x => x.Code)
            .ToListAsync();
    }

    public async Task<List<HrDepartment>> GetAllActiveAsync()
    {
        var query = await GetQueryableAsync();
        return await query
            .Include(x => x.Parent)
            .Include(x => x.Org)
            .Include(x => x.Type)
            .Include(x => x.Partner)
            .Include(x => x.Province)
            .Include(x => x.Ward)
            .Include(x => x.Bank)
            .Where(x => !x.IsDeleted && x.Status == HrDepartmentStatus.Active)
            .OrderBy(x => x.Code)
            .ToListAsync();
    }

    public async Task<List<Guid>> GetSelfAndDescendantIdsAsync(Guid departmentId)
    {
        var query = await GetQueryableAsync();
        var idToParentId = await query
            .Where(x => !x.IsDeleted)
            .Select(x => new { x.Id, x.ParentId })
            .ToListAsync();
        var byParent = idToParentId
            .Where(x => x.ParentId.HasValue)
            .GroupBy(x => x.ParentId!.Value)
            .ToDictionary(g => g.Key, g => g.Select(x => x.Id).ToList());
        var result = new List<Guid> { departmentId };
        var queue = new Queue<Guid>(new[] { departmentId });
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (byParent.TryGetValue(current, out var children))
            {
                foreach (var child in children)
                {
                    result.Add(child);
                    queue.Enqueue(child);
                }
            }
        }
        return result;
    }
}

