using System;
using System.ComponentModel.DataAnnotations;
using iOne.ProCoverageTypes;
using Volo.Abp.Application.Dtos;

namespace iOne.Product.ProCoverageTypes;

public class ProCoverageTypeDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "ProCoverageType:Code")]
    public string Code { get; set; } = null!;

    [Display(Name = "ProCoverageType:Name")]
    public string Name { get; set; } = null!;

    [Display(Name = "ProCoverageType:Description")]
    public string? Description { get; set; }

    [Display(Name = "ProCoverageType:Status")]
    public ProCoverageTypeStatus Status { get; set; }
}

