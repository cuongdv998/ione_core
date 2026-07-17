using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace iOne.File.Conversion;

public interface IFileConversionAppService : IApplicationService
{
    /// <summary>
    /// Converts a Word document (.doc / .docx) to PDF bytes.
    /// </summary>
    Task<byte[]> ConvertWordToPdfAsync(byte[] wordContent, string fileName, CancellationToken cancellationToken = default);
}
