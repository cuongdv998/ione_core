using iOne.PolicyTypes;
using Volo.Abp.Application.Dtos;

namespace iOne.Policy.PolicyTypes;

public class GetPolicyTypesInput : PagedAndSortedResultRequestDto
{
    public string? Code { get; set; }
    
    public string? Name { get; set; }
    
    public PolicyTypeStatus? Status { get; set; }
}
