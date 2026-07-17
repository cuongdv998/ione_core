using System;
using iOne.ProProductPlanDefinitions;
using Volo.Abp.Application.Dtos;

namespace iOne.Product.ProProductPlanDefinitions;

public class GetProProductPlanDefinitionsInput : PagedAndSortedResultRequestDto
{
    public string? PlanCode { get; set; }
    
    public string? PlanName { get; set; }
    
    public ProProductPlanDefinitionStatus? Status { get; set; }
    
    public Guid? ProductId { get; set; }
}
    