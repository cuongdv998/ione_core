using iOne.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace iOne.Report.ReportTemplateSqls
{
    public class ReportTemplateSqlAppService
    : CrudAppService<
        ReportTemplateSql,
        ReportTemplateSqlDto,
        Guid,
        PagedAndSortedResultRequestDto,
        CreateReportTemplateSqlDto,
        UpdateReportTemplateSqlDto>
    {
        public ReportTemplateSqlAppService(
            IRepository<ReportTemplateSql, Guid> repository)
            : base(repository)
        {
        }
    }
}
