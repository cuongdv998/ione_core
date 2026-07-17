using System;

namespace iOne.Master.ResDocuments;

public class FileResponseDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = null!;
    public string? Url { get; set; }
    public string MimeType { get; set; } = null!;
    public long FileSize { get; set; }
}
