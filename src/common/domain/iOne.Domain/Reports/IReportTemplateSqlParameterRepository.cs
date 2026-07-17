using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace iOne.Reports
{
    public interface IReportTemplateSqlParameterRepository : IRepository<ReportTemplateSqlParameter>
    {
        Task<List<ReportTemplateSqlParameter>> GetListBySqlIdsAsync(List<Guid> sqlIds);
        Task<List<ReportTemplateSqlParameter>> GetListByParameterIdAsync(Guid parameterId);
        Task<List<ReportTemplateSqlParameter>> GetListBySqlIdAsync(Guid sqlId);
    }
}
