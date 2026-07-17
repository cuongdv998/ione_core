using System;

namespace iOne.Product.ProTableRateLines;

public class ImportProTableRateLineErrorDto
{
    public int RowNumber { get; set; }
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
