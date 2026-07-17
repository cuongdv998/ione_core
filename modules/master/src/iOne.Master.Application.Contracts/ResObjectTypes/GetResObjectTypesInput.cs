using iOne.ResObjectTypes;
using Volo.Abp.Application.Dtos;

namespace iOne.Master.ResObjectTypes;

public class GetResObjectTypesInput : PagedAndSortedResultRequestDto
{
    public string? Code { get; set; }
    
    public string? Name { get; set; }
    
    public ResObjectGroup? ObjectGroup { get; set; }
    
    public ResObjectTypeStatus? Status { get; set; }
}

