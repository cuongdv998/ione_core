using System;
using System.Collections.Generic;

namespace iOne.Policy.Payments;

/// <summary>
/// Kết quả import thanh toán từ Excel
/// </summary>
public class ImportPaymentExcelResultDto
{
    public int TotalRows { get; set; }
    public int SuccessCount { get; set; }
    public int ErrorCount { get; set; }
    public List<ImportPaymentExcelErrorDto> Errors { get; set; } = new();
}

public class ImportPaymentExcelErrorDto
{
    public int RowNumber { get; set; }
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Value { get; set; }
}
