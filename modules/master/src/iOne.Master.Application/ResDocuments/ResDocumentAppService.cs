using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using iOne.Master;
using iOne.Master.Localization;
using iOne.Master.Permissions;
using iOne.Master.ResDocuments;
using iOne.ResDocuments;
using iOne.ResDocumentTypes;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace iOne.Master.ResDocuments;

[RemoteService(Name = MasterRemoteServiceConsts.RemoteServiceName)]
[Authorize(ResDocumentPermissions.Default)]
public class ResDocumentAppService : ApplicationService, IResDocumentAppService
{
    protected IResDocumentRepository DocumentRepository { get; }
    protected IMinIOService MinIOService { get; }
    protected IResDocumentTypeRepository DocumentTypeRepository { get; }
    protected MinIOOptions MinIOOptions { get; }

    public ResDocumentAppService(
        IResDocumentRepository documentRepository,
        IMinIOService minIOService,
        IResDocumentTypeRepository documentTypeRepository,
        IOptions<MinIOOptions> minIOOptions)
    {
        DocumentRepository = documentRepository;
        MinIOService = minIOService;
        DocumentTypeRepository = documentTypeRepository;
        MinIOOptions = minIOOptions.Value;
        LocalizationResource = typeof(MasterResource);
    }

    public virtual async Task<ResDocumentDto> UploadSingleFileAsync(
        UploadFileDto input,
        byte[] fileBytes,
        string fileName,
        string contentType)
    {
        await CheckPolicyAsync(ResDocumentPermissions.Create);

        if (fileBytes == null || fileBytes.Length == 0)
        {
            throw new UserFriendlyException("File cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new UserFriendlyException("File name is required.");
        }

        // Get DocumentType to retrieve bucket name
        var documentType = await DocumentTypeRepository.GetAsync(input.DocumentTypeId);
        if (string.IsNullOrWhiteSpace(documentType.Bucket))
        {
            throw new UserFriendlyException($"Document type '{documentType.Name}' does not have a bucket configured.");
        }

        var bucketName = documentType.Bucket;

        // Generate store file name
        var storeFileName = GenerateStoreFileName(fileName);
        var objectName = $"{input.DocumentTypeId}/{storeFileName}";

        // Resize image if large than 1MB
        var processed = await ResizeImageIfNecessaryAsync(fileBytes, contentType);
        fileBytes = processed.bytes;
        contentType = processed.contentType;

        // Compute checksum
        var checksum = ComputeChecksum(fileBytes);

        // Upload to MinIO
        using var stream = new MemoryStream(fileBytes);
        await MinIOService.UploadFileAsync(bucketName, objectName, stream, contentType);

        // Store fixed URL in database (not presigned URL)
        var baseUrl = MinIOOptions.FileBaseUrl.TrimEnd('/');
        var url = $"{baseUrl}/view/file/{input.DocumentTypeId}/{storeFileName}";

        // Create entity
        var entity = new ResDocument(
            GuidGenerator.Create(),
            input.DocumentTypeId,
            fileBytes.LongLength,
            fileName,
            storeFileName,
            contentType,
            bucketName,
            input.GroupCode,
            url,
            null, // thumbnailUrl
            checksum,
            null // versionId
        );

        await DocumentRepository.InsertAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();

        return ObjectMapper.Map<ResDocument, ResDocumentDto>(entity);
    }

    public virtual async Task<List<ResDocumentDto>> UploadMultipleFilesAsync(
        UploadMultipleFilesDto input,
        List<(byte[] fileBytes, string fileName, string contentType)> files)
    {
        await CheckPolicyAsync(ResDocumentPermissions.Create);

        if (files == null || files.Count == 0)
        {
            throw new UserFriendlyException("No files provided.");
        }

        // Get DocumentType to retrieve bucket name
        var documentType = await DocumentTypeRepository.GetAsync(input.DocumentTypeId);
        if (string.IsNullOrWhiteSpace(documentType.Bucket))
        {
            throw new UserFriendlyException($"Document type '{documentType.Name}' does not have a bucket configured.");
        }

        var bucketName = documentType.Bucket;
        var results = new List<ResDocumentDto>();

        foreach (var (fileBytes, fileName, contentType) in files)
        {
            if (fileBytes == null || fileBytes.Length == 0)
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(fileName))
            {
                continue;
            }

            try
            {
                // Generate store file name
                var storeFileName = GenerateStoreFileName(fileName);
                var objectName = $"{input.DocumentTypeId}/{storeFileName}";

                // Resize image if large than 1MB
                var processed = await ResizeImageIfNecessaryAsync(fileBytes, contentType);
                var currentBytes = processed.bytes;
                var currentContentType = processed.contentType;

                // Compute checksum
                var checksum = ComputeChecksum(currentBytes);

                // Upload to MinIO
                using var stream = new MemoryStream(currentBytes);
                await MinIOService.UploadFileAsync(bucketName, objectName, stream, currentContentType);

                // Store fixed URL in database (not presigned URL)
                var baseUrl = MinIOOptions.FileBaseUrl.TrimEnd('/');
                var url = $"{baseUrl}/view/file/{input.DocumentTypeId}/{storeFileName}";

                // Create entity
                var entity = new ResDocument(
                    GuidGenerator.Create(),
                    input.DocumentTypeId,
                    currentBytes.LongLength,
                    fileName,
                    storeFileName,
                    currentContentType,
                    bucketName,
                    input.GroupCode,
                    url,
                    null, // thumbnailUrl
                    checksum,
                    null // versionId
                );

                await DocumentRepository.InsertAsync(entity);
                results.Add(ObjectMapper.Map<ResDocument, ResDocumentDto>(entity));
            }
            catch (Exception ex)
            {
                // Log error but continue with other files
                Logger.LogWarning($"Failed to upload file {fileName}: {ex.Message}");
            }
        }

        await CurrentUnitOfWork.SaveChangesAsync();

        return results;
    }

    public virtual async Task DeleteSingleFileAsync(Guid id)
    {
        await CheckPolicyAsync(ResDocumentPermissions.Delete);

        var entity = await DocumentRepository.GetAsync(id);

        // Delete from MinIO using bucket from entity
        var objectName = $"{entity.DocumentTypeId}/{entity.StoreFileName}";
        await MinIOService.DeleteFileAsync(entity.BucketName, objectName);

        // Delete from database
        await DocumentRepository.DeleteAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    public virtual async Task DeleteMultipleFilesAsync(List<Guid> ids)
    {
        await CheckPolicyAsync(ResDocumentPermissions.Delete);

        if (ids == null || ids.Count == 0)
        {
            throw new UserFriendlyException("No file IDs provided.");
        }

        var entities = await DocumentRepository.GetListAsync(x => ids.Contains(x.Id));

        foreach (var entity in entities)
        {
            try
            {
                // Delete from MinIO using bucket from entity
                var objectName = $"{entity.DocumentTypeId}/{entity.StoreFileName}";
                await MinIOService.DeleteFileAsync(entity.BucketName, objectName);

                // Delete from database
                await DocumentRepository.DeleteAsync(entity);
            }
            catch (Exception ex)
            {
                // Log error but continue with other files
                Logger.LogWarning($"Failed to delete file {entity.FileName}: {ex.Message}");
            }
        }

        await CurrentUnitOfWork.SaveChangesAsync();
    }

    public virtual async Task<FileResponseDto> GetSingleFileAsync(Guid id)
    {
        await CheckPolicyAsync(ResDocumentPermissions.View);

        var entity = await DocumentRepository.GetAsync(id);

        return new FileResponseDto
        {
            Id = entity.Id,
            FileName = entity.FileName,
            Url = BuildViewUrl(entity.DocumentTypeId, entity.StoreFileName, entity.Url),
            MimeType = entity.MimeType,
            FileSize = entity.FileSize
        };
    }


    public virtual async Task<List<FileResponseDto>> GetMultipleFilesAsync(List<Guid> ids)
    {
        await CheckPolicyAsync(ResDocumentPermissions.View);

        if (ids == null || ids.Count == 0)
        {
            throw new UserFriendlyException("No file IDs provided.");
        }

        var entities = await DocumentRepository.GetListAsync(x => ids.Contains(x.Id));
        var results = new List<FileResponseDto>();

        foreach (var entity in entities)
        {
            try
            {
                results.Add(new FileResponseDto
                {
                    Id = entity.Id,
                    FileName = entity.FileName,
                    Url = BuildViewUrl(entity.DocumentTypeId, entity.StoreFileName, entity.Url),
                    MimeType = entity.MimeType,
                    FileSize = entity.FileSize
                });
            }
            catch (Exception ex)
            {
                // Log error but continue with other files
                Logger.LogWarning($"Failed to get file {entity.FileName}: {ex.Message}");
            }
        }

        await CurrentUnitOfWork.SaveChangesAsync();

        return results;
    }

    public virtual async Task<PagedResultDto<ResDocumentDto>> GetListAsync(GetResDocumentsInput input)
    {
        await CheckPolicyAsync(ResDocumentPermissions.View);

        var query = await DocumentRepository.GetQueryableAsync();

        // Filter by DocumentTypeId
        if (input.DocumentTypeId.HasValue)
        {
            query = query.Where(x => x.DocumentTypeId == input.DocumentTypeId.Value);
        }

        // Filter by GroupCode
        if (!string.IsNullOrWhiteSpace(input.GroupCode))
        {
            query = query.Where(x => x.GroupCode == input.GroupCode);
        }

        var totalCount = await AsyncExecuter.CountAsync(query);

        var entities = await AsyncExecuter.ToListAsync(query);
        var dtos = ObjectMapper.Map<List<ResDocument>, List<ResDocumentDto>>(entities);

        return new PagedResultDto<ResDocumentDto>(totalCount, dtos);
    }

    [AllowAnonymous]
    public virtual async Task<ViewFileResponseDto> ViewFileAsync(Guid documentTypeId, string storeFileName, string date)
    {
        // Find document by DocumentTypeId and StoreFileName
        var pathFile = date + "/" + storeFileName;
        var query = await DocumentRepository.GetQueryableAsync();
        var entity = await AsyncExecuter.FirstOrDefaultAsync(
            query.Where(x => x.DocumentTypeId == documentTypeId && x.StoreFileName == pathFile));

        if (entity == null)
        {
            throw new UserFriendlyException("File not found.");
        }

        // Get file stream from MinIO
        var objectName = $"{entity.DocumentTypeId}/{entity.StoreFileName}";
        var fileStream = await MinIOService.GetFileAsync(entity.BucketName, objectName);

        return new ViewFileResponseDto
        {
            FileStream = fileStream,
            FileName = entity.FileName,
            ContentType = entity.MimeType
        };
    }

    private string BuildViewUrl(Guid documentTypeId, string? storeFileName, string? fallbackUrl)
    {
        if (string.IsNullOrWhiteSpace(storeFileName))
        {
            return fallbackUrl ?? string.Empty;
        }

        var normalized = storeFileName.Replace('\\', '/');
        var separatorIndex = normalized.IndexOf('/');
        if (separatorIndex <= 0 || separatorIndex >= normalized.Length - 1)
        {
            return fallbackUrl ?? string.Empty;
        }

        var datePart = normalized[..separatorIndex];
        var namePart = normalized[(separatorIndex + 1)..];
        var baseUrl = (MinIOOptions.FileBaseUrl ?? string.Empty).TrimEnd('/');
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            return fallbackUrl ?? string.Empty;
        }

        return $"{baseUrl}/view/file/{documentTypeId}/{datePart}/{namePart}";
    }

    private string GenerateStoreFileName(string originalFileName)
    {
        var extension = Path.GetExtension(originalFileName);
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(originalFileName);
        var dateFolder = DateTime.UtcNow.ToString("yyyyMMdd");
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var randomString = Guid.NewGuid().ToString("N")[..8];

        // Sanitize file name
        var sanitized = string.Join("_", fileNameWithoutExtension.Split(Path.GetInvalidFileNameChars()));

        // Return format: yyyyMMdd/filename
        return $"{dateFolder}/{sanitized}_{timestamp}_{randomString}{extension}";
    }

    private string ComputeChecksum(byte[] data)
    {
        using var sha1 = SHA1.Create();
        var hashBytes = sha1.ComputeHash(data);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }

    private async Task<(byte[] bytes, string contentType)> ResizeImageIfNecessaryAsync(byte[] fileBytes, string contentType)
    {
        if (fileBytes.Length <= 1024 * 1024)
        {
            return (fileBytes, contentType);
        }

        if (string.IsNullOrWhiteSpace(contentType) ||
            !contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase) ||
            contentType.Equals("image/svg+xml", StringComparison.OrdinalIgnoreCase))
        {
            return (fileBytes, contentType);
        }

        try
        {
            using var inputStream = new MemoryStream(fileBytes);
            using var image = await Image.LoadAsync(inputStream);

            // A typical max dimension for web images
            const int maxDimension = 2000;
            if (image.Width > maxDimension || image.Height > maxDimension)
            {
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = new Size(maxDimension, maxDimension),
                    Mode = ResizeMode.Max
                }));
            }

            using var outputStream = new MemoryStream();

            // Try different qualities to get under 1MB
            int quality = 85;
            await image.SaveAsJpegAsync(outputStream, new JpegEncoder { Quality = quality });

            while (outputStream.Length > 1024 * 1024 && quality > 20)
            {
                outputStream.SetLength(0);
                quality -= 10;
                await image.SaveAsJpegAsync(outputStream, new JpegEncoder { Quality = quality });
            }

            return (outputStream.ToArray(), "image/jpeg");
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"Failed to resize image: {ex.Message}. Using original file.");
            return (fileBytes, contentType);
        }
    }
}
