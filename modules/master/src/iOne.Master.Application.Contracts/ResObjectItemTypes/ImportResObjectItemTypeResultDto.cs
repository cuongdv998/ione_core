using System.Collections.Generic;

namespace iOne.Master.ResObjectItemTypes;

public class ImportResObjectItemTypeResultDto
{
    public int TotalRows { get; set; }
    
    public int SuccessCount { get; set; }
    
    public int ErrorCount { get; set; }
    
    public List<ImportResObjectItemTypeErrorDto> Errors { get; set; } = new();
}

public class ImportResObjectItemTypeErrorDto
{
    public int RowNumber { get; set; }
    
    public string Field { get; set; } = null!;
    
    public string Message { get; set; } = null!;
    
    public string? Value { get; set; }
}
