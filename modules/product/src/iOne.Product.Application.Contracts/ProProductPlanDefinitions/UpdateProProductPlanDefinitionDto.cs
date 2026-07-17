using System;
using System.ComponentModel.DataAnnotations;
using iOne.ProProductPlanDefinitions;

namespace iOne.Product.ProProductPlanDefinitions;

public class UpdateProProductPlanDefinitionDto
{
    // ⚠️ QUAN TRỌNG: Không có PlanCode property - PlanCode không được phép sửa

    [Required(ErrorMessage = "ProProductPlanDefinition:ProductIdRequired")]
    [Display(Name = "ProProductPlanDefinition:ProductId")]
    public Guid ProductId { get; set; }

    [Required(ErrorMessage = "ProProductPlanDefinition:PlanNameRequired")]
    [StringLength(250, ErrorMessage = "ProProductPlanDefinition:PlanNameMaxLength")]
    [Display(Name = "ProProductPlanDefinition:PlanName")]
    public string PlanName { get; set; } = null!;

    [Required(ErrorMessage = "ProProductPlanDefinition:StatusRequired")]
    [Display(Name = "ProProductPlanDefinition:Status")]
    public ProProductPlanDefinitionStatus Status { get; set; }
}
