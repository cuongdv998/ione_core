using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using iOne.EntityFrameworkCore;
using iOne.ResDocumentTypes;

namespace iOne.EntityFrameworkCore.ResDocumentTypes;

public class EfCoreResDocumentTypeRepository : EfCoreRepository<iOneDbContext, ResDocumentType, Guid>, IResDocumentTypeRepository
{
    public EfCoreResDocumentTypeRepository(IDbContextProvider<iOneDbContext> dbContextProvider)
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

    public async Task<ResDocumentType?> FindByCodeAsync(string code)
    {
        var query = await GetQueryableAsync();
        return await query.FirstOrDefaultAsync(x => x.Code == code);
    }

    public async Task<List<ResDocumentType>> GetListByDocumentGroupCodeAsync(string documentGroupCode)
    {
        var normalized = documentGroupCode.Trim();
        var query = await GetQueryableAsync();

        return await query
            .Where(x => x.DocumentGroupCode != null && x.DocumentGroupCode == normalized)
            .OrderBy(x => x.Name)
            .ToListAsync();
    }
}
