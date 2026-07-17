using iOne.Report.Permissions;
using iOne.Report.ReportTemplates;
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

namespace iOne.Report.Controllers
{
    //[RemoteService(Name = ReportRemoteServiceConsts.RemoteServiceName)]
    //[Area(ReportRemoteServiceConsts.ModuleName)]
    //[Route("api/report-template/sql")]
    //[Authorize(ReportTemplateSqlPermissions.Default)]
    public class ReportTemplateSqlController
    {
        //protected IReportTemplateSqlAppService AppService { get; }

        //public ReportTemplateSqlController(IReportTemplateSqlAppService appService)
        //{
        //    AppService = appService;
        //}

        //[HttpGet]
        //[Authorize(ReportTemplatePermissions.View)]
        //public virtual Task<PagedResultDto<ReportTemplateSqlDto>> GetListAsync([FromBody] PagedAndSortedResultRequestDto input)
        //{
        //    return AppService.GetListAsync(input);
        //}

        //[HttpGet("{id}")]

        //[Authorize(ReportTemplatePermissions.View)]
        //public virtual Task<ReportTemplateSqlDto> GetAsync(Guid id)
        //{
        //    return AppService.GetAsync(id);
        //}

        //[HttpPost]

        //[Authorize(ReportTemplatePermissions.Create)]
        //public virtual Task<ReportTemplateSqlDto> CreateAsync(CreateReportTemplateSqlDto input)
        //{
        //    return AppService.CreateAsync(input);
        //}

        //[HttpPut("{id}")]

        //[Authorize(ReportTemplatePermissions.Edit)]
        //public virtual Task<ReportTemplateSqlDto> UpdateAsync(Guid id, UpdateReportTemplateSqlDto input)
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
