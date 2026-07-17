using System;
using System.ComponentModel.DataAnnotations;
using iOne.ProProductPlanDefinitions;

namespace iOne.Product.ProProductPlanDefinitions;

public class CreateProProductPlanDefinitionDto
{
    [Required(ErrorMessage = "ProProductPlanDefinition:ProductIdRequired")]
    [Display(Name = "ProProductPlanDefinition:ProductId")]
    public Guid ProductId { get; set; }

    [Required(ErrorMessage = "ProProductPlanDefinition:PlanCodeRequired")]
    [StringLength(50, ErrorMessage = "ProProductPlanDefinition:PlanCodeMaxLength")]
    [RegularExpression(@"^[A-Z0-9_]+$", ErrorMessage = "ProProductPlanDefinition:PlanCodeInvalidFormat")]
    [Display(Name = "ProProductPlanDefinition:PlanCode")]
    public string PlanCode { get; set; } = null!;

    [Required(ErrorMessage = "ProProductPlanDefinition:PlanNameRequired")]
    [StringLength(250, ErrorMessage = "ProProductPlanDefinition:PlanNameMaxLength")]
    [Display(Name = "ProProductPlanDefinition:PlanName")]
    public string PlanName { get; set; } = null!;

    [Required(ErrorMessage = "ProProductPlanDefinition:StatusRequired")]
    [Display(Name = "ProProductPlanDefinition:Status")]
    public ProProductPlanDefinitionStatus Status { get; set; } = ProProductPlanDefinitionStatus.Active;
}
