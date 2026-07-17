using System;
using System.ComponentModel.DataAnnotations;
using iOne.ProProducts;
using Volo.Abp.Application.Dtos;

namespace iOne.Product.ProProducts;

public class ProProductCoverageInteractionDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "ProProductCoverageInteraction:ProductCoverageId")]
    public Guid ProductCoverageId { get; set; }

    [Display(Name = "ProProductCoverageInteraction:InteractionType")]
    public ProProductCoverageInteractionType InteractionType { get; set; }

    // String-friendly value for FE display/filtering (e.g. "dependency", "incompatible", "exclusive")
    public string? InteractionTypeCode { get; set; }

    [Display(Name = "ProProductCoverageInteraction:InteractionCoverageId")]
    public Guid InteractionCoverageId { get; set; }

    [Display(Name = "ProProductCoverageInteraction:EffectDate")]
    public DateTime EffectDate { get; set; }

    [Display(Name = "ProProductCoverageInteraction:ExpireDate")]
    public DateTime? ExpireDate { get; set; }

    [Display(Name = "ProProductCoverageInteraction:CreatorName")]
    public string? CreatorName { get; set; }
}
