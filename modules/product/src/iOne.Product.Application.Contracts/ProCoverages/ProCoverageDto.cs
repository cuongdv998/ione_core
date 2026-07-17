using System;
using System.ComponentModel.DataAnnotations;
using iOne.ProCoverages;
using Volo.Abp.Application.Dtos;

namespace iOne.Product.ProCoverages;

public class ProCoverageDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "ProCoverage:LobId")]
    public Guid LobId { get; set; }

    [Display(Name = "ProCoverage:ObjectTypeId")]
    public Guid? ObjectTypeId { get; set; }

    [Display(Name = "ProCoverage:CoverageGroupId")]
    public Guid CoverageGroupId { get; set; }

    [Display(Name = "ProCoverage:CoverageTypeId")]
    public Guid? CoverageTypeId { get; set; }

    [Display(Name = "ProCoverage:Code")]
    public string Code { get; set; } = null!;

    [Display(Name = "ProCoverage:ShortName")]
    public string ShortName { get; set; } = null!;

    [Display(Name = "ProCoverage:Name")]
    public string Name { get; set; } = null!;

    [Display(Name = "ProCoverage:Type")]
    public ProCoverageTermType Type { get; set; }

    [Display(Name = "ProCoverage:Description")]
    public string? Description { get; set; }

    [Display(Name = "ProCoverage:Status")]
    public ProCoverageStatus Status { get; set; }
}
