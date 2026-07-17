using System;
using Volo.Abp.Application.Dtos;

namespace iOne.Policy.PolicyVersions;

public class GetPolicyVersionsInput : PagedAndSortedResultRequestDto
{
    public Guid? PolicyId { get; set; }
    
    public string? Type { get; set; }
    
    public string? Status { get; set; }
}
