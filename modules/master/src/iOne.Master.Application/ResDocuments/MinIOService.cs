using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Minio;
using Minio.Exceptions;
using Volo.Abp;

namespace iOne.Master.ResDocuments;

public class MinIOService : IMinIOService, IDisposable
{
    private readonly MinIOOptions _options;
    private readonly IMinioClient _minioClient;
    private bool _bucketExistsChecked = false;
    private readonly SemaphoreSlim _bucketCheckSemaphore = new SemaphoreSlim(1, 1);
    private readonly ILogger<MinIOService> _logger;

    public MinIOService(IOptions<MinIOOptions> options, ILogger<MinIOService> logger)
    {
        _options = options.Value;
        _logger = logger;

        var endpoint = _options.Endpoint.Trim();
        var useSSL = _options.UseSSL;

        _minioClient = new MinioClient()
            .WithEndpoint(endpoint)
            .WithCredentials(_options.AccessKey, _options.SecretKey)
            .WithSSL(useSSL)
            .Build();
    }

    private async Task EnsureBucketExistsAsync()
    {
        if (_bucketExistsChecked)
            return;

        await _bucketCheckSemaphore.WaitAsync();
        try
        {
            if (_bucketExistsChecked)
                return;

            var bucketExistsArgs = new BucketExistsArgs()
                .WithBucket(_options.BucketName);
            
            bool found = await _minioClient.BucketExistsAsync(bucketExistsArgs);
            
            if (!found)
            {
                var makeBucketArgs = new MakeBucketArgs()
                    .WithBucket(_options.BucketName);
                
                await _minioClient.MakeBucketAsync(makeBucketArgs);
            }

            _bucketExistsChecked = true;
        }
        catch (Exception ex)
        {
            throw new BusinessException($"Failed to ensure MinIO bucket exists: {ex.Message}");
        }
        finally
        {
            _bucketCheckSemaphore.Release();
        }
    }

    public async Task<string> UploadFileAsync(string bucketName, string objectName, Stream fileStream, string contentType)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(bucketName))
            {
                throw new ArgumentException("Bucket name cannot be null or empty.", nameof(bucketName));
            }

            // Ensure bucket exists
            await CreateBucketAsync(bucketName);
            
            // Reset stream position to beginning
            if (fileStream.CanSeek)
            {
                fileStream.Position = 0;
            }

            var putObjectArgs = new PutObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName)
                .WithStreamData(fileStream)
                .WithObjectSize(fileStream.Length)
                .WithContentType(contentType);

            await _minioClient.PutObjectAsync(putObjectArgs);

            return objectName;
        }
        catch (Exception ex)
        {
            // Log chi tiết lỗi để debug
            _logger.LogError("MinIO upload failed. Bucket: {Bucket}, Object: {Object}, ContentType: {ContentType}",
                bucketName, objectName, contentType);

            // Giữ inner exception
            throw new BusinessException($"Failed to upload file to MinIO: {ex.Message}");
        }
    }

    public async Task<Stream> GetFileAsync(string bucketName, string objectName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(bucketName))
            {
                throw new ArgumentException("Bucket name cannot be null or empty.", nameof(bucketName));
            }

            var memoryStream = new MemoryStream();
            
            var getObjectArgs = new GetObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName)
                .WithCallbackStream(stream =>
                {
                    stream.CopyTo(memoryStream);
                });

            await _minioClient.GetObjectAsync(getObjectArgs);
            
            memoryStream.Position = 0;
            return memoryStream;
        }
        catch (ObjectNotFoundException)
        {
            throw new BusinessException($"File not found: {objectName}");
        }
        catch (Exception ex)
        {
            throw new BusinessException($"Failed to get file from MinIO: {ex.Message}");
        }
    }

    public async Task<bool> DeleteFileAsync(string bucketName, string objectName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(bucketName))
            {
                throw new ArgumentException("Bucket name cannot be null or empty.", nameof(bucketName));
            }

            var removeObjectArgs = new RemoveObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName);

            await _minioClient.RemoveObjectAsync(removeObjectArgs);
            return true;
        }
        catch (Exception ex)
        {
            throw new BusinessException($"Failed to delete file from MinIO: {ex.Message}");
        }
    }

    public async Task<string> GetFileUrlAsync(string bucketName, string objectName, int expiryInSeconds = 3600)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(bucketName))
            {
                throw new ArgumentException("Bucket name cannot be null or empty.", nameof(bucketName));
            }

            var presignedGetObjectArgs = new PresignedGetObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName)
                .WithExpiry(expiryInSeconds);

            return await _minioClient.PresignedGetObjectAsync(presignedGetObjectArgs);
        }
        catch (Exception ex)
        {
            throw new BusinessException($"Failed to get file URL from MinIO: {ex.Message}");
        }
    }

    public async Task<bool> FileExistsAsync(string bucketName, string objectName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(bucketName))
            {
                throw new ArgumentException("Bucket name cannot be null or empty.", nameof(bucketName));
            }

            var statObjectArgs = new StatObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName);

            await _minioClient.StatObjectAsync(statObjectArgs);
            return true;
        }
        catch (ObjectNotFoundException)
        {
            return false;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> CreateBucketAsync(string bucketName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(bucketName))
            {
                throw new ArgumentException("Bucket name cannot be null or empty.", nameof(bucketName));
            }

            // Validate bucket name format (MinIO requirements: lowercase, 3-63 chars, alphanumeric, dots, hyphens)
            var normalizedBucketName = bucketName.ToLowerInvariant().Trim();
            if (normalizedBucketName.Length < 3 || normalizedBucketName.Length > 63)
            {
                throw new ArgumentException("Bucket name must be between 3 and 63 characters.", nameof(bucketName));
            }

            // Check if bucket already exists
            var bucketExistsArgs = new BucketExistsArgs()
                .WithBucket(normalizedBucketName);
            
            bool exists = await _minioClient.BucketExistsAsync(bucketExistsArgs);
            
            if (exists)
            {
                return true; // Bucket already exists
            }

            // Create the bucket
            var makeBucketArgs = new MakeBucketArgs()
                .WithBucket(normalizedBucketName);
            
            await _minioClient.MakeBucketAsync(makeBucketArgs);
            
            return true;
        }
        catch (Exception ex)
        {
            throw new BusinessException($"Failed to create MinIO bucket '{bucketName}': {ex.Message}");
        }
    }

    public void Dispose()
    {
        _bucketCheckSemaphore?.Dispose();
        _minioClient?.Dispose();
    }
}
