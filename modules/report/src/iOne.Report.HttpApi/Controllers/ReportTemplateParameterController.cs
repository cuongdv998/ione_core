using iOne.Report.Permissions;
using iOne.Report.ReportTemplateParameters;
using iOne.Report.ReportTemplateSqls;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;

namespace iOne.Report.Controllers
{
    //[RemoteService(Name = ReportRemoteServiceConsts.RemoteServiceName)]
    //[Area(ReportRemoteServiceConsts.ModuleName)]
    //[Route("api/report-template/parameter")]
    //[Authorize(ReportTemplateParameterPermissions.Default)]
    public class ReportTemplateParameterController : AbpControllerBase
    {
        //protected IReportTemplateParameterAppService AppService { get; }

        //public ReportTemplateParameterController(IReportTemplateParameterAppService appService)
        //{
        //    AppService = appService;
        //}

        //[HttpGet]
        //[Authorize(ReportTemplatePermissions.View)]
        //public virtual Task<PagedResultDto<ReportTemplateParameterDto>> GetListAsync([FromBody] PagedAndSortedResultRequestDto input)
        //{
        //    return AppService.GetListAsync(input);
        //}

        //[HttpGet("{id}")]

        //[Authorize(ReportTemplatePermissions.View)]
        //public virtual Task<ReportTemplateParameterDto> GetAsync(Guid id)
        //{
        //    return AppService.GetAsync(id);
        //}

        //[HttpPost]

        //[Authorize(ReportTemplatePermissions.Create)]
        //public virtual Task<ReportTemplateParameterDto> CreateAsync(CreateReportTemplateParameterDto input)
        //{
        //    return AppService.CreateAsync(input);
        //}

        //[HttpPut("{id}")]

        //[Authorize(ReportTemplatePermissions.Edit)]
        //public virtual Task<ReportTemplateParameterDto> UpdateAsync(Guid id, UpdateReportTemplateParameterDto input)
        //{
        //    return AppService.UpdateAsync(id, input);
        //}

        //[HttpDelete("{id}")]

        //[Authorize(ReportTemplatePermissions.Delete)]
        //public virtual Task DeleteAsync(Guid id)
        //{
        //    return AppService.DeleteAsync(id);
        //}
    }
}
