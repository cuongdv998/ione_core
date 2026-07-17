using iOne.ResReasonGroups;
using Volo.Abp.Application.Dtos;

namespace iOne.Master.ResReasonGroups;

public class GetResReasonGroupsInput : PagedAndSortedResultRequestDto
{
    public string? Code { get; set; }
    
    public string? Name { get; set; }
    
    public ResReasonGroupStatus? Status { get; set; }
}
