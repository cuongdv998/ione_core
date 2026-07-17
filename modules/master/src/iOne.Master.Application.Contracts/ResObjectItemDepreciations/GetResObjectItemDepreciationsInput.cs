using System;
using iOne.ResObjectItemDepreciations;
using Volo.Abp.Application.Dtos;

namespace iOne.Master.ResObjectItemDepreciations;

public class GetResObjectItemDepreciationsInput : PagedAndSortedResultRequestDto
{
    public Guid? ObjectTypeItemId { get; set; }
    
    public Guid? CarGroupId { get; set; }
    
    public ResObjectItemDepreciationStatus? Status { get; set; }
    
    public DateTime? EffectDateFrom { get; set; }
    
    public DateTime? EffectDateTo { get; set; }
}
