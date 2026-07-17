using iOne.ResEvents;
using Volo.Abp.Application.Dtos;

namespace iOne.Master.ResEvents;

public class GetResEventsInput : PagedAndSortedResultRequestDto
{
    public string? Code { get; set; }
    
    public string? Name { get; set; }
    
    public ResEventStatus? Status { get; set; }
}

