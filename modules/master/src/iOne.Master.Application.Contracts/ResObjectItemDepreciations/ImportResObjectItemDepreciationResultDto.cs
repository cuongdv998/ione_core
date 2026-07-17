using System.Collections.Generic;

namespace iOne.Master.ResObjectItemDepreciations;

public class ImportResObjectItemDepreciationResultDto
{
    public int TotalRows { get; set; }
    
    public int SuccessCount { get; set; }
    
    public int ErrorCount { get; set; }
    
    public List<ImportResObjectItemDepreciationErrorDto> Errors { get; set; } = new();
}

public class ImportResObjectItemDepreciationErrorDto
{
    public int RowNumber { get; set; }
    
    public string Field { get; set; } = null!;
    
    public string Message { get; set; } = null!;
    
    public string? Value { get; set; }
}
