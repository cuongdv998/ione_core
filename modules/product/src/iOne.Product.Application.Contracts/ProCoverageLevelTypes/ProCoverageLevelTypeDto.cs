using System;
using System.ComponentModel.DataAnnotations;
using iOne.ProCoverageLevelTypes;
using Volo.Abp.Application.Dtos;

namespace iOne.Product.ProCoverageLevelTypes;

public class ProCoverageLevelTypeDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "ProCoverageLevelType:Code")]
    public string Code { get; set; } = null!;

    [Display(Name = "ProCoverageLevelType:Name")]
    public string Name { get; set; } = null!;

    [Display(Name = "ProCoverageLevelType:Description")]
    public string? Description { get; set; }

    [Display(Name = "ProCoverageLevelType:Status")]
    public ProCoverageLevelTypeStatus Status { get; set; }
}
