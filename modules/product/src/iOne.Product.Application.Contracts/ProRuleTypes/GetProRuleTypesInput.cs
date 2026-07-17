using iOne.ProRuleTypes;
using Volo.Abp.Application.Dtos;

namespace iOne.Product.ProRuleTypes;

public class GetProRuleTypesInput : PagedAndSortedResultRequestDto
{
    public string? Code { get; set; }
    
    public string? Name { get; set; }
    
    public ProRuleTypeStatus? Status { get; set; }
}
