using iOne.Report.ReportTemplateParameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Content;

namespace iOne.Report.ReportTemplates
{
    public interface IReportTemplateAppService : ICrudAppService<ReportTemplateDto, Guid, FilterDto, CreateReportTemplateDto, UpdateReportTemplateDto>
    {
        Task<ReportTemplateDetailDto> GetDetailAsync(Guid id);

        Task<IRemoteStreamContent> ExportAsync(FilterDto input);

        Task<IRemoteStreamContent> ExportDynamicFile(Guid templateId, Dictionary<string, object> parameters);

        Task<IRemoteStreamContent> ExportDynamicFileByCode(string code, Dictionary<string, object> parameters);

        Task<List<ReportTemplateParameterDto>> GetParameterAsync(Guid reportTemplateId);

        Task<List<ReportTemplateDDL>> GetTemplateDdlsAsync();
    }
}
