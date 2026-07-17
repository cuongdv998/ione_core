using System;
using System.ComponentModel.DataAnnotations;
using iOne.ProCoverageLevelBases;
using Volo.Abp.Application.Dtos;

namespace iOne.Product.ProCoverageLevelBasis;

public class ProCoverageLevelBasisDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "ProCoverageLevelBasis:Code")]
    public string Code { get; set; } = null!;

    [Display(Name = "ProCoverageLevelBasis:Name")]
    public string Name { get; set; } = null!;

    [Display(Name = "ProCoverageLevelBasis:Description")]
    public string? Description { get; set; }

    [Display(Name = "ProCoverageLevelBasis:Status")]
    public ProCoverageLevelBasisStatus Status { get; set; }
}
