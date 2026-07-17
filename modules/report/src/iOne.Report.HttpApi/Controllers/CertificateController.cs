using iOne.Report.Certificates;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Content;

namespace iOne.Report.Controllers
{
    [Route("api/report/certificate")]
    public class CertificateController : iOneReportController
    {
        private readonly ICertificateAppService _certificateAppService;

        public CertificateController(ICertificateAppService certificateAppService)
        {
            _certificateAppService = certificateAppService;
        }

        [HttpPost]
        [Route("generate")]
        public virtual Task<IRemoteStreamContent> GenerateCertificateAsync([FromQuery] Guid documentId, [FromBody] Dictionary<string, object> data)
        {
            return _certificateAppService.GenerateCertificateAsync(documentId, data);
        }
    }
}
