using iOne.ResCarLines;
using Volo.Abp.Application.Dtos;

namespace iOne.Master.ResCarLines;

public class GetResCarLinesInput : PagedAndSortedResultRequestDto
{
    public string? Code { get; set; }
    
    public string? Name { get; set; }
    
    public ResCarLineStatus? Status { get; set; }
}


