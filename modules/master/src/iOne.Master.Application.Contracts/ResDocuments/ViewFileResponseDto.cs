using System.IO;

namespace iOne.Master.ResDocuments;

public class ViewFileResponseDto
{
    public Stream FileStream { get; set; } = null!;
    public string FileName { get; set; } = null!;
    public string ContentType { get; set; } = null!;
}
