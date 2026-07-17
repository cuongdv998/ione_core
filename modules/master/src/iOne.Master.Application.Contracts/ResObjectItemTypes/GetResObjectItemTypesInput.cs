using iOne.ResObjectItemTypes;
using Volo.Abp.Application.Dtos;

namespace iOne.Master.ResObjectItemTypes;

public class GetResObjectItemTypesInput : PagedAndSortedResultRequestDto
{
    public string? Code { get; set; }
    
    public string? Name { get; set; }
    
    public ResObjectItemTypeStatus? Status { get; set; }
}
