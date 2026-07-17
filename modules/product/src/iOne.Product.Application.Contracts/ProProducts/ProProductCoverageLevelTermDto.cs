using System;
using System.ComponentModel.DataAnnotations;
using iOne.ProCoverageLevelTypes;
using Volo.Abp.Application.Dtos;

namespace iOne.Product.ProProducts;

public class ProProductCoverageLevelTermDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "ProProductCoverageLevelTerm:ProductCoverageLevelId")]
    public Guid? ProductCoverageLevelId { get; set; }

    [Display(Name = "ProProductCoverageLevelTerm:CoverageLevelTypeId")]
    public Guid CoverageLevelTypeId { get; set; }

    // Flattened join from pro_coverage_level_type (optional in response)
    [Display(Name = "ProCoverageLevelType:Code")]
    public string? CoverageLevelTypeCode { get; set; }

    [Display(Name = "ProCoverageLevelType:Name")]
    public string? CoverageLevelTypeName { get; set; }

    [Display(Name = "ProCoverageLevelType:Description")]
    public string? CoverageLevelTypeDescription { get; set; }

    [Display(Name = "ProCoverageLevelType:Status")]
    public ProCoverageLevelTypeStatus? CoverageLevelTypeStatus { get; set; }

    [Display(Name = "ProProductCoverageLevelTerm:CoverageLevelBasisId")]
    public Guid? CoverageLevelBasisId { get; set; }

    [Display(Name = "ProProductCoverageLevelTerm:AmountType")]
    public string AmountType { get; set; } = null!;

    [Display(Name = "ProProductCoverageLevelTerm:FromAmount")]
    public decimal FromAmount { get; set; }

    [Display(Name = "ProProductCoverageLevelTerm:ToAmount")]
    public decimal ToAmount { get; set; }

    [Display(Name = "ProProductCoverageLevelTerm:ConditionScript")]
    public string? ConditionScript { get; set; }

    [Display(Name = "ProProductCoverageLevelTerm:ComputeScript")]
    public string? ComputeScript { get; set; }

    [Display(Name = "ProProductCoverageLevelTerm:IsDefault")]
    public string? IsDefault { get; set; }

    [Display(Name = "ProProductCoverageLevelTerm:CreatorName")]
    public string? CreatorName { get; set; }
}
