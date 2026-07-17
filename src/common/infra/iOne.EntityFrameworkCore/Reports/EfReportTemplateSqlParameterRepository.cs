using iOne.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;


namespace iOne.Reports
{
    public class EfReportTemplateSqlParameterRepository : EfCoreRepository<iOneDbContext, ReportTemplateSqlParameter>, IReportTemplateSqlParameterRepository
    {
        public EfReportTemplateSqlParameterRepository(IDbContextProvider<iOneDbContext> dbContextProvider)
        : base(dbContextProvider)
        {
        }

        public async Task<List<ReportTemplateSqlParameter>> GetListBySqlIdsAsync(List<Guid> sqlIds)
        {
            return await (await GetQueryableAsync())
                .Where(x => sqlIds.Contains(x.SqlId)).ToListAsync();
        }

        public async Task<List<ReportTemplateSqlParameter>> GetListByParameterIdAsync(Guid parameterId)
        {
            return await (await GetQueryableAsync())
                .Where(x => x.ParameterId == parameterId).ToListAsync();
        }

        public async Task<List<ReportTemplateSqlParameter>> GetListBySqlIdAsync(Guid sqlId)
        {
            return await (await GetQueryableAsync())
                .Where(x => x.SqlId == sqlId).ToListAsync();
        }
    }
}
