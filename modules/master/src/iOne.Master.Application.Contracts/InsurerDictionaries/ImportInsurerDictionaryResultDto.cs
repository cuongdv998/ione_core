namespace iOne.Master.InsurerDictionaries;

/// <summary>
/// Result of Insurer Dictionary Excel import. FileBytes is the Excel file with error messages in the last column for failed rows.
/// </summary>
public class ImportInsurerDictionaryResultDto
{
    public int TotalRows { get; set; }
    public int SuccessCount { get; set; }
    public int ErrorCount { get; set; }
    /// <summary>
    /// Excel file with "Lỗi" column filled for failed rows. Base64-encoded for JSON response.
    /// </summary>
    public string? FileBase64 { get; set; }
    public string? FileName { get; set; }
}
