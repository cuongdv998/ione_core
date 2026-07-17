using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace iOne.Reports
{
    public interface IReportTemplateSqlRepository : IRepository<ReportTemplateSql, Guid>
    {
        Task<List<ReportTemplateSql>> GetListByTemplateIdAsync(Guid templateId);
    }
}
