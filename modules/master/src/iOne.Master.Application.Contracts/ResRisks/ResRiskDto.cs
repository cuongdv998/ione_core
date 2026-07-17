using System;
using System.ComponentModel.DataAnnotations;
using iOne.ResRisks;
using Volo.Abp.Application.Dtos;

namespace iOne.Master.ResRisks;

public class ResRiskDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "ResRisk:ObjectTypeId")]
    public Guid ObjectTypeId { get; set; }

    [Display(Name = "ResRisk:Code")]
    public string Code { get; set; } = null!;

    [Display(Name = "ResRisk:Name")]
    public string Name { get; set; } = null!;

    [Display(Name = "ResRisk:Description")]
    public string? Description { get; set; }

    [Display(Name = "ResRisk:Status")]
    public ResRiskStatus Status { get; set; }
}

