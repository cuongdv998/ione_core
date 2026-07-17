using System;
using iOne.ResRisks;
using Volo.Abp.Application.Dtos;

namespace iOne.Master.ResRisks;

public class GetResRisksInput : PagedAndSortedResultRequestDto
{
    public Guid? ObjectTypeId { get; set; }
    
    public string? Code { get; set; }
    
    public string? Name { get; set; }
    
    public ResRiskStatus? Status { get; set; }
}

