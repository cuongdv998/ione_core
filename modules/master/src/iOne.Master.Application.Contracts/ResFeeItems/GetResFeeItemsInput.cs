using iOne.ResFeeItems;
using Volo.Abp.Application.Dtos;

namespace iOne.Master.ResFeeItems;

public class GetResFeeItemsInput : PagedAndSortedResultRequestDto
{
    public string? Code { get; set; }
    
    public string? Name { get; set; }
    
    public ResFeeItemStatus? Status { get; set; }
}
