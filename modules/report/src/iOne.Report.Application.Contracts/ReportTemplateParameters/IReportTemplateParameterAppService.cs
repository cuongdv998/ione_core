using iOne.Report.ReportTemplates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace iOne.Report.ReportTemplateParameters
{
    public interface IReportTemplateParameterAppService : ICrudAppService<ReportTemplateParameterDto, Guid, PagedAndSortedResultRequestDto, CreateReportTemplateParameterDto, UpdateReportTemplateParameterDto>
    {
    }
}
