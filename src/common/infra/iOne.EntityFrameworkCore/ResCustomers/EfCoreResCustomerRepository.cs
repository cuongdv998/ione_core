using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using iOne.EntityFrameworkCore;
using iOne.ResCustomers;

namespace iOne.EntityFrameworkCore.ResCustomers;

public class EfCoreResCustomerRepository : EfCoreRepository<iOneDbContext, ResCustomer, Guid>, IResCustomerRepository
{
    public EfCoreResCustomerRepository(IDbContextProvider<iOneDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<bool> AnyByIndustryIdAsync(Guid industryId)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet.AnyAsync(x => !x.IsDeleted && x.IndustryId == industryId);
    }

    public async Task<bool> AnyByProvinceIdAsync(Guid provinceId)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet.AnyAsync(x => !x.IsDeleted && (x.ProvinceId == provinceId));
    }

    public async Task<bool> AnyByWardIdAsync(Guid wardId)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet.AnyAsync(x => !x.IsDeleted && (x.WardId == wardId));
    }

    public async Task<bool> AnyByOrganizationTypeIdAsync(Guid organizationTypeId)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet.AnyAsync(x => !x.IsDeleted && x.OrganizationTypeId == organizationTypeId);
    }
}

