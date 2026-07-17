using System.Collections.Generic;

namespace iOne.Master.ResCarModels;

public class ImportResCarModelResultDto
{
    public int TotalRows { get; set; }
    
    public int SuccessCount { get; set; }
    
    public int ErrorCount { get; set; }
    
    public List<ImportResCarModelErrorDto> Errors { get; set; } = new();
}

public class ImportResCarModelErrorDto
{
    public int RowNumber { get; set; }
    
    public string Field { get; set; } = null!;
    
    public string Message { get; set; } = null!;
    
    public string? Value { get; set; }
}



