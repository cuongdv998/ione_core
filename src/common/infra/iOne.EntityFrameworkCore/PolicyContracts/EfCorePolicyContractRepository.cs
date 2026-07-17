using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using iOne.EntityFrameworkCore;
using iOne.PolicyContracts;

namespace iOne.EntityFrameworkCore.PolicyContracts;

public class EfCorePolicyContractRepository : EfCoreRepository<iOneDbContext, PolicyContract, Guid>, IPolicyContractRepository
{
    public EfCorePolicyContractRepository(IDbContextProvider<iOneDbContext> dbContextProvider)
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

    public async Task<bool> IsInsurerContractCodeExistsAsync(Guid? insurerId, string insurerContractCode, Guid? excludeId = null)
    {
        if (!insurerId.HasValue || string.IsNullOrWhiteSpace(insurerContractCode))
        {
            return false;
        }

        var query = await GetQueryableAsync();
        query = query.Where(x => x.InsurerId == insurerId.Value && x.InsurerContractCode == insurerContractCode.Trim());

        if (excludeId.HasValue)
        {
            query = query.Where(x => x.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }
}
