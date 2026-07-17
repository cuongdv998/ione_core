using iOne.Report.Permissions;
using iOne.Report.ReportTemplateParameters;
using iOne.Report.ReportTemplates;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc.AntiForgery;
using Volo.Abp.Content;


namespace iOne.Report.Controllers
{
    [RemoteService(Name = ReportRemoteServiceConsts.RemoteServiceName)]
    [Area(ReportRemoteServiceConsts.ModuleName)]
    [Route("api/report-template/template")]
    [Authorize(ReportTemplatePermissions.Default)]
    public class ReportTemplateController : AbpControllerBase
    {
        protected IReportTemplateAppService AppService { get; }

        public ReportTemplateController(IReportTemplateAppService appService)
        {
            AppService = appService;
        }

        [HttpPost("get-all")]
        [Authorize(ReportTemplatePermissions.View)]
        public virtual Task<PagedResultDto<ReportTemplateDto>> GetListAsync([FromBody] FilterDto input)
        {
            return AppService.GetListAsync(input);
        }

        [HttpGet("ddl")]
        [Authorize(ReportTemplatePermissions.View)]
        public virtual Task<List<ReportTemplateDDL>> GetDdlAsync()
        {
            return AppService.GetTemplateDdlsAsync();
        }

        [HttpGet("list-params/{id}")]
        [Authorize(ReportTemplatePermissions.View)]
        public virtual Task<List<ReportTemplateParameterDto>> GetListParamsAsync(Guid id)
        {
            return AppService.GetParameterAsync(id);
        }

        [HttpGet("detail/{id}")]
        [Authorize(ReportTemplatePermissions.View)]
        public virtual Task<ReportTemplateDetailDto> GetDetailAsync(Guid id)
        {
            return AppService.GetDetailAsync(id);
        }

        [HttpGet("{id}")]

        [Authorize(ReportTemplatePermissions.View)]
        public virtual Task<ReportTemplateDto> GetAsync(Guid id)
        {
            return AppService.GetAsync(id);
        }

        [HttpPost]

        [Authorize(ReportTemplatePermissions.Create)]
        public virtual Task<ReportTemplateDto> CreateAsync(CreateReportTemplateDto input)
        {
            return AppService.CreateAsync(input);
        }

        [HttpPut("{id}")]

        [Authorize(ReportTemplatePermissions.Edit)]
        public virtual Task<ReportTemplateDto> UpdateAsync(Guid id, UpdateReportTemplateDto input)
        {
            return AppService.UpdateAsync(id, input);
        }

        [HttpDelete("{id}")]

        [Authorize(ReportTemplatePermissions.Delete)]
        public virtual Task DeleteAsync(Guid id)
        {
            return AppService.DeleteAsync(id);
        }


        [Authorize(ReportTemplatePermissions.View)]
        [HttpPost("export")]
        public async Task<IRemoteStreamContent> ExportAsync(FilterDto input)
        {
            return await AppService.ExportAsync(input);
        }

        [Authorize(ReportTemplatePermissions.View)]
        [HttpPost("export-dynamic/{id}")]
        public async Task<IRemoteStreamContent> GenDynamicFileAsync(Guid id, Dictionary<string, object> parameters)
        {
            return await AppService.ExportDynamicFile(id, parameters);
        }

        [Authorize(ReportTemplatePermissions.View)]
        [HttpPost("export-dynamic-by-code/{code}")]
        public async Task<IRemoteStreamContent> GenDynamicFileByCodeAsync(string code, Dictionary<string, object> parameters)
        {
            return await AppService.ExportDynamicFileByCode(code, parameters);
        }
    }
}
