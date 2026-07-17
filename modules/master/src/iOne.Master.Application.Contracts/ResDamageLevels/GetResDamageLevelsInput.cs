using System;
using iOne.ResDamageLevels;
using Volo.Abp.Application.Dtos;

namespace iOne.Master.ResDamageLevels;

public class GetResDamageLevelsInput : PagedAndSortedResultRequestDto
{
    public Guid? ObjectTypeId { get; set; }
    
    public string? Code { get; set; }
    
    public string? Name { get; set; }
    
    public ResDamageLevelStatus? Status { get; set; }
}

