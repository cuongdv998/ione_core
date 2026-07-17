using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using iOne.Product.ProRules;
using iOne.Product.ProProductPlanDefinitions;
using iOne.Product.ProProductDistributions;
using iOne.ProProducts;
using Volo.Abp.Application.Dtos;

namespace iOne.Product.ProProducts;

public class ProProductDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "ProProduct:ProductTypeId")]
    public Guid? ProductTypeId { get; set; }

    /// <summary>Mã loại sản phẩm (từ pro_product_type), dùng so sánh VCX/TNDS.</summary>
    public string? ProductTypeCode { get; set; }

    [Display(Name = "ProProduct:PartnerId")]
    public Guid? PartnerId { get; set; }

    [Display(Name = "ProProduct:TableRateId")]
    public Guid? TableRateId { get; set; }

    [Display(Name = "ProProduct:RootProductId")]
    public Guid? RootProductId { get; set; }

    [Display(Name = "ProProduct:IsRootProduct")]
    public string IsRootProduct { get; set; } = "Y";

    [Display(Name = "ProProduct:LobId")]
    public Guid LobId { get; set; }

    [Display(Name = "ProProduct:ProductCategoryId")]
    public Guid? ProductCategoryId { get; set; }

    [Display(Name = "ProProduct:CurrencyId")]
    public Guid? CurrencyId { get; set; }

    [Display(Name = "ProProduct:Code")]
    public string Code { get; set; } = null!;

    [Display(Name = "ProProduct:InsurerProductCode")]
    public string? InsurerProductCode { get; set; }

    [Display(Name = "ProProduct:ShortName")]
    public string ShortName { get; set; } = null!;

    [Display(Name = "ProProduct:Name")]
    public string Name { get; set; } = null!;

    [Display(Name = "ProProduct:Description")]
    public string? Description { get; set; }

    [Display(Name = "ProProduct:InternalNote")]
    public string? InternalNote { get; set; }

    [Display(Name = "ProProduct:RateType")]
    public string RateType { get; set; } = "table_rate";

    [Display(Name = "ProProduct:Status")]
    public ProProductStatus Status { get; set; }

    [Display(Name = "ProProduct:SeqNumber")]
    public int SeqNumber { get; set; } = 1;

    [Display(Name = "ProProduct:EffectDate")]
    public DateTime EffectDate { get; set; }

    [Display(Name = "ProProduct:ExpireDate")]
    public DateTime? ExpireDate { get; set; }

    [Display(Name = "ProProduct:PlanDefinitionId")]
    public Guid? PlanDefinitionId { get; set; }

    [Display(Name = "ProProduct:IsPlan")]
    public string? IsPlan { get; set; }

    [Display(Name = "ProProduct:ImageDocumentId")]
    public Guid? ImageDocumentId { get; set; }

    [Display(Name = "ProProduct:CertificateTemplateDocumentId")]
    public Guid? CertificateTemplateDocumentId { get; set; }

    [Display(Name = "ProProduct:ProductAttributes")]
    public List<ProProductAttributeDto> ProductAttributes { get; set; } = new List<ProProductAttributeDto>();

    [Display(Name = "ProProduct:ProductCoverages")]
    public List<ProProductCoverageDto> ProductCoverages { get; set; } = new List<ProProductCoverageDto>();

    [Display(Name = "ProProduct:ProductTableRates")]
    public List<ProProductTableRateDto> ProductTableRates { get; set; } = new List<ProProductTableRateDto>();

    [Display(Name = "ProProduct:ProRules")]
    public List<ProRuleDto> ProRules { get; set; } = new List<ProRuleDto>();

    [Display(Name = "ProProduct:ProductPlanDefinitions")]
    public List<ProProductPlanDefinitionDto> ProductPlanDefinitions { get; set; } = new List<ProProductPlanDefinitionDto>();

    [Display(Name = "ProProduct:ProductDistributions")]
    public List<ProProductDistributionDto> ProductDistributions { get; set; } = new List<ProProductDistributionDto>();
}
