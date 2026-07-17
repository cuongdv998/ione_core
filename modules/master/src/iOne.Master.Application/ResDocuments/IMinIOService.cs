using System.IO;
using System.Threading.Tasks;

namespace iOne.Master.ResDocuments;

public interface IMinIOService
{
    Task<string> UploadFileAsync(string bucketName, string objectName, Stream fileStream, string contentType);
    Task<Stream> GetFileAsync(string bucketName, string objectName);
    Task<bool> DeleteFileAsync(string bucketName, string objectName);
    Task<string> GetFileUrlAsync(string bucketName, string objectName, int expiryInSeconds = 3600);
    Task<bool> FileExistsAsync(string bucketName, string objectName);
    Task<bool> CreateBucketAsync(string bucketName);
}
