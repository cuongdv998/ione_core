using System;
using System.ComponentModel.DataAnnotations;
using iOne.ProProductPlanDefinitions;
using Volo.Abp.Application.Dtos;

namespace iOne.Product.ProProductPlanDefinitions;

public class ProProductPlanDefinitionDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "ProProductPlanDefinition:ProductId")]
    public Guid ProductId { get; set; }

    [Display(Name = "ProProductPlanDefinition:PlanCode")]
    public string PlanCode { get; set; } = null!;

    [Display(Name = "ProProductPlanDefinition:PlanName")]
    public string PlanName { get; set; } = null!;

    [Display(Name = "ProProductPlanDefinition:Status")]
    public ProProductPlanDefinitionStatus Status { get; set; }

    [Display(Name = "ProProductPlanDefinition:CreatorName")]
    public string? CreatorName { get; set; }
}
