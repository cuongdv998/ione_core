using System;
using iOne.ResReasonGroups;
using Volo.Abp.Application.Dtos;

namespace iOne.Master.ResReasons;

public class GetResReasonsInput : PagedAndSortedResultRequestDto
{
    public Guid? GroupId { get; set; }
    
    public string? Code { get; set; }
    
    public string? Name { get; set; }
    
    public ResReasonGroupStatus? Status { get; set; }
}
