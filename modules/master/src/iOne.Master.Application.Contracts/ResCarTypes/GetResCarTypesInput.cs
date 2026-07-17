using iOne.ResCarTypes;
using Volo.Abp.Application.Dtos;

namespace iOne.Master.ResCarTypes;

public class GetResCarTypesInput : PagedAndSortedResultRequestDto
{
    public string? Code { get; set; }
    
    public string? Name { get; set; }
    
    public ResCarTypeStatus? Status { get; set; }
}

