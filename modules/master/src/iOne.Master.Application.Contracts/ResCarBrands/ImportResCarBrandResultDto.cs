using System.Collections.Generic;

namespace iOne.Master.ResCarBrands;

public class ImportResCarBrandResultDto
{
    public int TotalRows { get; set; }
    
    public int SuccessCount { get; set; }
    
    public int ErrorCount { get; set; }
    
    public List<ImportResCarBrandErrorDto> Errors { get; set; } = new();
}

public class ImportResCarBrandErrorDto
{
    public int RowNumber { get; set; }
    
    public string Field { get; set; } = null!;
    
    public string Message { get; set; } = null!;
    
    public string? Value { get; set; }
}



