using System;
using System.ComponentModel.DataAnnotations;
using iOne.ProCoverageGroups;
using Volo.Abp.Application.Dtos;

namespace iOne.Product.ProCoverageGroups;

public class ProCoverageGroupDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "ProCoverageGroup:Code")]
    public string Code { get; set; } = null!;

    [Display(Name = "ProCoverageGroup:Name")]
    public string Name { get; set; } = null!;

    [Display(Name = "ProCoverageGroup:Status")]
    public ProCoverageGroupStatus Status { get; set; }

    [Display(Name = "ProCoverageGroup:Description")]
    public string? Description { get; set; }
}

