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
    public class EfReportTemplateParameterRepository : EfCoreRepository<iOneDbContext, ReportTemplateParameter, Guid>, IReportTemplateParameterRepository
    {
        public EfReportTemplateParameterRepository(IDbContextProvider<iOneDbContext> dbContextProvider) : base(dbContextProvider)
        {
        }

        public async Task<List<ReportTemplateParameter>> GetListByTemplateIdAsync(Guid templateId)
        {
            return await (await GetQueryableAsync())
                .Where(x => x.ReportTemplateId == templateId)
                .ToListAsync();
        }
    }
}
