using System.Collections.Generic;

namespace iOne.Master.ResCarGroups;

public class ImportResCarGroupResultDto
{
    public int TotalRows { get; set; }
    
    public int SuccessCount { get; set; }
    
    public int ErrorCount { get; set; }
    
    public List<ImportResCarGroupErrorDto> Errors { get; set; } = new();
}

public class ImportResCarGroupErrorDto
{
    public int RowNumber { get; set; }
    
    public string Field { get; set; } = null!;
    
    public string Message { get; set; } = null!;
    
    public string? Value { get; set; }
}
