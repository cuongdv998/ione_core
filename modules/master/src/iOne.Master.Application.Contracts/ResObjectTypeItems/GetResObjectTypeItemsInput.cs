using System;
using iOne.ResObjectTypeItems;
using Volo.Abp.Application.Dtos;

namespace iOne.Master.ResObjectTypeItems;

public class GetResObjectTypeItemsInput : PagedAndSortedResultRequestDto
{
    public string? Code { get; set; }
    
    public string? Name { get; set; }
    
    public Guid? ObjectTypeId { get; set; }
    
    public Guid? ObjectItemType { get; set; }
    
    public Guid? UomId { get; set; }
    
    public ResObjectTypeItemStatus? Status { get; set; }
}
