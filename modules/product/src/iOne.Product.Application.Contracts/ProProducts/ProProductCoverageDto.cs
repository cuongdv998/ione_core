using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using iOne.ProProducts;
using iOne.Product.ProCoverages;
using iOne.Product.ResTaxes;
using Volo.Abp.Application.Dtos;

namespace iOne.Product.ProProducts;

public class ProProductCoverageDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "ProProductCoverage:ProductId")]
    public Guid ProductId { get; set; }

    [Display(Name = "ProProductCoverage:CoverageId")]
    public Guid CoverageId { get; set; }

    [Display(Name = "ProProductCoverage:ParentId")]
    public Guid? ParentId { get; set; }

    [Display(Name = "ProProductCoverage:InsurerCoverageCode")]
    public string? InsurerCoverageCode { get; set; }

    [Display(Name = "ProProductCoverage:UomId")]
    public Guid? UomId { get; set; }

    [Display(Name = "ProProductCoverage:AvailabilityType")]
    public ProProductCoverageAvailabilityType AvailabilityType { get; set; }

    [Display(Name = "ProProductCoverage:EffectDate")]
    public DateTime EffectDate { get; set; }

    [Display(Name = "ProProductCoverage:ExpireDate")]
    public DateTime? ExpireDate { get; set; }

    [Display(Name = "ProProductCoverage:SeqNumber")]
    public int SeqNumber { get; set; } = 1;

    [Display(Name = "ProProductCoverage:TaxId")]
    public Guid TaxId { get; set; }

    [Display(Name = "ProProductCoverage:Tax")]
    public ResTaxDto? Tax { get; set; }

    [Display(Name = "ProProductCoverage:EnableQuantity")]
    public string? EnableQuantity { get; set; }

    [Display(Name = "ProProductCoverage:ProductCoverageInteractions")]
    public List<ProProductCoverageInteractionDto> ProductCoverageInteractions { get; set; } = new List<ProProductCoverageInteractionDto>();

    [Display(Name = "ProProductCoverage:ProductCoverageLevels")]
    public List<ProProductCoverageLevelDto> ProductCoverageLevels { get; set; } = new List<ProProductCoverageLevelDto>();

    [Display(Name = "ProProductCoverage:Coverage")]
    public ProCoverageDto? Coverage { get; set; }
}
