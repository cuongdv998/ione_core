using iOne.ResUomClasses;
using Volo.Abp.Application.Dtos;

namespace iOne.Master.ResUomClasses;

public class GetResUomClassesInput : PagedAndSortedResultRequestDto
{
    public string? Code { get; set; }
    
    public string? Name { get; set; }
    
    public ResUomClassStatus? Status { get; set; }
}

