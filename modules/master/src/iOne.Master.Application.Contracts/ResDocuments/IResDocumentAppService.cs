using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace iOne.Master.ResDocuments;

public interface IResDocumentAppService : IApplicationService
{
    Task<ResDocumentDto> UploadSingleFileAsync(UploadFileDto input, byte[] fileBytes, string fileName, string contentType);
    Task<List<ResDocumentDto>> UploadMultipleFilesAsync(UploadMultipleFilesDto input, List<(byte[] fileBytes, string fileName, string contentType)> files);
    Task DeleteSingleFileAsync(Guid id);
    Task DeleteMultipleFilesAsync(List<Guid> ids);
    Task<FileResponseDto> GetSingleFileAsync(Guid id);
    Task<List<FileResponseDto>> GetMultipleFilesAsync(List<Guid> ids);
    Task<PagedResultDto<ResDocumentDto>> GetListAsync(GetResDocumentsInput input);
    Task<ViewFileResponseDto> ViewFileAsync(Guid documentTypeId, string storeFileName, string date);
}
