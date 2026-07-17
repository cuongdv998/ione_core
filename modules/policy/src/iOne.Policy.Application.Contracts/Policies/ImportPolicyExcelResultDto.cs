using System.Collections.Generic;

namespace iOne.Policy.Policies;

/// <summary>
/// Kết quả import đơn bảo hiểm từ Excel
/// </summary>
public class ImportPolicyExcelResultDto
{
    public int TotalRows { get; set; }
    public int SuccessCount { get; set; }
    public int ErrorCount { get; set; }
    public List<ImportPolicyExcelErrorDto> Errors { get; set; } = new();
    /// <summary>
    /// Single lot import code for this Excel run; every policy created in the import shares this value.
    /// </summary>
    public string? ImportLotCode { get; set; }
    /// <summary>
    /// Per successfully imported row; <see cref="ImportPolicyLotImportCodeDto.LotImportCode"/> matches <see cref="ImportLotCode"/>.
    /// </summary>
    public List<ImportPolicyLotImportCodeDto> LotImportCodes { get; set; } = new();
    public string? ErrorFileName { get; set; }
    public string? ErrorFileBase64 { get; set; }
}

public class ImportPolicyLotImportCodeDto
{
    public int RowNumber { get; set; }
    public string LotImportCode { get; set; } = string.Empty;
}

public class ImportPolicyExcelErrorDto
{
    public int RowNumber { get; set; }
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Value { get; set; }
}
