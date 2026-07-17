using System;
using iOne.ResCarGroups;
using Volo.Abp.Application.Dtos;

namespace iOne.Master.ResCarGroups;

public class GetResCarGroupsInput : PagedAndSortedResultRequestDto
{
    public Guid? CarLineId { get; set; }
    
    public string? Code { get; set; }
    
    public string? Name { get; set; }
    
    public ResCarGroupStatus? Status { get; set; }
}
