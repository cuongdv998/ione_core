using iOne.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace iOne.Report.ReportTemplateParameters
{
    public class ReportTemplateParameterAppService : CrudAppService<ReportTemplateParameter, ReportTemplateParameterDto, Guid, PagedAndSortedResultRequestDto, CreateReportTemplateParameterDto, UpdateReportTemplateParameterDto>, IReportTemplateParameterAppService
    {
        public ReportTemplateParameterAppService(IRepository<ReportTemplateParameter, Guid> repository) : base(repository)
        {
        }
    }
}
