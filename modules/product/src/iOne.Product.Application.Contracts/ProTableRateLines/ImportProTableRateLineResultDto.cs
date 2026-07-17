using System;
using System.Collections.Generic;

namespace iOne.Product.ProTableRateLines;

public class ImportProTableRateLineResultDto
{
    public int TotalRows { get; set; }
    public int SuccessCount { get; set; }
    public int ErrorCount { get; set; }
    public byte[] ResultFileBytes { get; set; } = Array.Empty<byte>();
    public List<ImportProTableRateLineErrorDto> Errors { get; set; } = new();
}
