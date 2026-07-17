using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;
using iOne.Master.ResDocuments;
using Volo.Abp;

namespace iOne.Master.Controllers;

[RemoteService(Name = MasterRemoteServiceConsts.RemoteServiceName)]
[Area(MasterRemoteServiceConsts.ModuleName)]
[Route("")] // Root level route
public class FileViewController : AbpControllerBase
{
    protected IResDocumentAppService AppService { get; }

    public FileViewController(IResDocumentAppService appService)
    {
        AppService = appService;
    }

    [HttpGet("view/file/{documentTypeId}/{date}/{storeFileName}")]
    [AllowAnonymous]
    public virtual async Task<IActionResult> ViewFileAsync(
        Guid documentTypeId,
        string storeFileName,
        string date,
        [FromQuery] bool inline = false)
    {
        var result = await AppService.ViewFileAsync(documentTypeId, storeFileName, date);
        if (inline)
        {
            return File(result.FileStream, result.ContentType);
        }

        return File(result.FileStream, result.ContentType, result.FileName);
    }
}
