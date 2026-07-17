using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;
using iOne.Master.Permissions;
using iOne.Master.ResDocuments;

namespace iOne.Master.Controllers;

[RemoteService(Name = MasterRemoteServiceConsts.RemoteServiceName)]
[Area(MasterRemoteServiceConsts.ModuleName)]
[Route("api/master/documents")]
public class ResDocumentController : AbpControllerBase
{
    protected IResDocumentAppService AppService { get; }

    public ResDocumentController(IResDocumentAppService appService)
    {
        AppService = appService;
    }

    [HttpPost("upload-single")]
    [Authorize(ResDocumentPermissions.Create)]
    [Consumes("multipart/form-data")]
    public virtual async Task<ResDocumentDto> UploadSingleFileAsync(
        [FromQuery] Guid documentTypeId,
        [FromQuery] string? groupCode,
        IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            throw new UserFriendlyException("File is required.");
        }

        byte[] fileBytes;
        using (var memoryStream = new System.IO.MemoryStream())
        {
            await file.CopyToAsync(memoryStream);
            fileBytes = memoryStream.ToArray();
        }

        var input = new UploadFileDto
        {
            DocumentTypeId = documentTypeId,
            GroupCode = groupCode
        };

        return await AppService.UploadSingleFileAsync(
            input,
            fileBytes,
            file.FileName,
            file.ContentType);
    }

    [HttpPost("upload-multiple")]
    [Authorize(ResDocumentPermissions.Create)]
    [Consumes("multipart/form-data")]
    public virtual async Task<List<ResDocumentDto>> UploadMultipleFilesAsync(
        [FromQuery] Guid documentTypeId,
        [FromQuery] string? groupCode,
        List<IFormFile> files)
    {
        if (files == null || files.Count == 0)
        {
            throw new UserFriendlyException("At least one file is required.");
        }

        var fileData = new List<(byte[] fileBytes, string fileName, string contentType)>();

        foreach (var file in files)
        {
            if (file != null && file.Length > 0)
            {
                byte[] fileBytes;
                using (var memoryStream = new System.IO.MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    fileBytes = memoryStream.ToArray();
                }

                fileData.Add((fileBytes, file.FileName, file.ContentType));
            }
        }

        if (fileData.Count == 0)
        {
            throw new UserFriendlyException("No valid files provided.");
        }

        var input = new UploadMultipleFilesDto
        {
            DocumentTypeId = documentTypeId,
            GroupCode = groupCode
        };

        return await AppService.UploadMultipleFilesAsync(input, fileData);
    }

    [HttpDelete("{id}")]
    [Authorize(ResDocumentPermissions.Delete)]
    public virtual Task DeleteSingleFileAsync(Guid id)
    {
        return AppService.DeleteSingleFileAsync(id);
    }

    [HttpPost("delete-multiple")]
    [Authorize(ResDocumentPermissions.Delete)]
    public virtual Task DeleteMultipleFilesAsync([FromBody] List<Guid> ids)
    {
        return AppService.DeleteMultipleFilesAsync(ids);
    }

    [HttpGet("{id}")]
    [Authorize(ResDocumentPermissions.View)]
    public virtual Task<FileResponseDto> GetSingleFileAsync(Guid id)
    {
        return AppService.GetSingleFileAsync(id);
    }

    [HttpPost("get-multiple")]
    [Authorize(ResDocumentPermissions.View)]
    public virtual Task<List<FileResponseDto>> GetMultipleFilesAsync([FromBody] List<Guid> ids)
    {
        return AppService.GetMultipleFilesAsync(ids);
    }

    [HttpGet]
    [Authorize(ResDocumentPermissions.View)]
    public virtual Task<PagedResultDto<ResDocumentDto>> GetListAsync(GetResDocumentsInput input)
    {
        return AppService.GetListAsync(input);
    }
}
