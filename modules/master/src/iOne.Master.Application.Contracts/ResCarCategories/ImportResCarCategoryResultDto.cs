using System.Collections.Generic;

namespace iOne.Master.ResCarCategories;

public class ImportResCarCategoryResultDto
{
    public int TotalRows { get; set; }
    
    public int SuccessCount { get; set; }
    
    public int ErrorCount { get; set; }
    
    public List<ImportResCarCategoryErrorDto> Errors { get; set; } = new();
}

public class ImportResCarCategoryErrorDto
{
    public int RowNumber { get; set; }
    
    public string Field { get; set; } = null!;
    
    public string Message { get; set; } = null!;
    
    public string? Value { get; set; }
}


