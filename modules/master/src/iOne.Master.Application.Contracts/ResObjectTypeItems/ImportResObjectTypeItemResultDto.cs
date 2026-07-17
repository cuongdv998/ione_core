using System.Collections.Generic;

namespace iOne.Master.ResObjectTypeItems;

public class ImportResObjectTypeItemResultDto
{
    public int TotalRows { get; set; }
    
    public int SuccessCount { get; set; }
    
    public int ErrorCount { get; set; }
    
    public List<ImportResObjectTypeItemErrorDto> Errors { get; set; } = new();
}

public class ImportResObjectTypeItemErrorDto
{
    public int RowNumber { get; set; }
    
    public string Field { get; set; } = null!;
    
    public string Message { get; set; } = null!;
    
    public string? Value { get; set; }
}
