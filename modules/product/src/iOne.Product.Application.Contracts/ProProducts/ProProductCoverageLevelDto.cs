using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace iOne.Product.ProProducts;

public class ProProductCoverageLevelDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "ProProductCoverageLevel:ProductCoverageId")]
    public Guid? ProductCoverageId { get; set; }

    [Display(Name = "ProProductCoverageLevel:Code")]
    public string Code { get; set; } = null!;

    [Display(Name = "ProProductCoverageLevel:Name")]
    public string Name { get; set; } = null!;

    [Display(Name = "ProProductCoverageLevel:EffectDate")]
    public DateTime EffectDate { get; set; }

    [Display(Name = "ProProductCoverageLevel:ExpireDate")]
    public DateTime? ExpireDate { get; set; }

    [Display(Name = "ProProductCoverageLevel:ConditionalScript")]
    public string? ConditionalScript { get; set; }

    [Display(Name = "ProProductCoverageLevel:Terms")]
    public List<ProProductCoverageLevelTermDto>? Terms { get; set; }

    [Display(Name = "ProProductCoverageLevel:CreatorName")]
    public string? CreatorName { get; set; }
}
