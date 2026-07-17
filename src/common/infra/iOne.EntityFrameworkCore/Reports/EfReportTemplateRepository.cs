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
    public class EfReportTemplateRepository : EfCoreRepository<iOneDbContext, ReportTemplate, Guid>, IReportTemplateRepository
    {
        public EfReportTemplateRepository(IDbContextProvider<iOneDbContext> dbContextProvider)
        : base(dbContextProvider)
        {
        }
    }
}
