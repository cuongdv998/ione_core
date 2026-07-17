using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Content;

namespace iOne.Report.Certificates
{
    public interface ICertificateAppService : IApplicationService
    {
        Task<IRemoteStreamContent> GenerateCertificateAsync(Guid documentId, Dictionary<string, object> data);
    }
}
