using System;
using System.ComponentModel.DataAnnotations;
using iOne.ResRisks;

namespace iOne.Master.ResRisks;

public class UpdateResRiskDto
{
    // ⚠️ QUAN TRỌNG: Không có Code property - Code không được phép sửa

    [Required(ErrorMessage = "ResRisk:ObjectTypeIdRequired")]
    [Display(Name = "ResRisk:ObjectTypeId")]
    public Guid ObjectTypeId { get; set; }

    [Required(ErrorMessage = "ResRisk:NameRequired")]
    [StringLength(250, ErrorMessage = "ResRisk:NameMaxLength")]
    [Display(Name = "ResRisk:Name")]
    public string Name { get; set; } = null!;

    [StringLength(500, ErrorMessage = "ResRisk:DescriptionMaxLength")]
    [Display(Name = "ResRisk:Description")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "ResRisk:StatusRequired")]
    [Display(Name = "ResRisk:Status")]
    public ResRiskStatus Status { get; set; }
}

