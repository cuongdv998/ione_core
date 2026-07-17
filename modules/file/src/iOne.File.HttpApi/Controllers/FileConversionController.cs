using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using iOne.File.Conversion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;

namespace iOne.File.Controllers;

[RemoteService(Name = FileRemoteServiceConsts.RemoteServiceName)]
[Area(FileRemoteServiceConsts.ModuleName)]
[Route("api/file/conversion")]
[Authorize]
public class FileConversionController : AbpControllerBase
{
    protected IFileConversionAppService AppService { get; }

    public FileConversionController(IFileConversionAppService appService)
    {
        AppService = appService;
    }

    /// <summary>
    /// Upload a Word file (.doc / .docx); response body is the PDF.
    /// </summary>
    [HttpPost("word-to-pdf")]
    [Consumes("multipart/form-data")]
    public virtual async Task<IActionResult> WordToPdfAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest();

        await using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        var pdfBytes = await AppService.ConvertWordToPdfAsync(ms.ToArray(), file.FileName ?? "document.docx");

        var baseName = Path.GetFileNameWithoutExtension(file.FileName ?? "document");
        if (string.IsNullOrWhiteSpace(baseName))
        {
            baseName = "document";
        }

        var downloadName = baseName + ".pdf";
        Response.Headers.ContentDisposition = BuildUtf8ContentDisposition(downloadName);
        return new FileContentResult(pdfBytes, "application/pdf");
    }

    /// <summary>
    /// RFC 5987 <c>filename*</c> so Vietnamese names are not mangled to underscores in the browser.
    /// </summary>
    private static string BuildUtf8ContentDisposition(string fileName)
    {
        var asciiFallback = new string(fileName.Select(c => c <= 0x7f ? c : '_').ToArray());
        var star = Uri.EscapeDataString(fileName);
        return $"attachment; filename=\"{asciiFallback.Replace("\"", "\\\"")}\"; filename*=UTF-8''{star}";
    }
}
