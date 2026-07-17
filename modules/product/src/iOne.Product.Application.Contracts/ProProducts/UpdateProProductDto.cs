using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using iOne.ProProducts;
using iOne.Product.ProRules;
using iOne.Product.ProProductPlanDefinitions;
using iOne.Product.ProProductDistributions;

namespace iOne.Product.ProProducts;

public class UpdateProProductDto
{
    // ⚠️ QUAN TRỌNG: Không có Code property - Code không được phép sửa

    [Display(Name = "ProProduct:ProductTypeId")]
    public Guid? ProductTypeId { get; set; }

    [Display(Name = "ProProduct:PartnerId")]
    public Guid? PartnerId { get; set; }

    [Display(Name = "ProProduct:TableRateId")]
    public Guid? TableRateId { get; set; }

    [Display(Name = "ProProduct:RootProductId")]
    public Guid? RootProductId { get; set; }

    [Display(Name = "ProProduct:IsRootProduct")]
    [StringLength(1, ErrorMessage = "ProProduct:IsRootProductMaxLength")]
    public string IsRootProduct { get; set; } = "Y";

    [Required(ErrorMessage = "ProProduct:LobIdRequired")]
    [Display(Name = "ProProduct:LobId")]
    public Guid LobId { get; set; }

    [Display(Name = "ProProduct:ProductCategoryId")]
    public Guid? ProductCategoryId { get; set; }

    [Display(Name = "ProProduct:CurrencyId")]
    public Guid? CurrencyId { get; set; }

    [StringLength(50, ErrorMessage = "ProProduct:InsurerProductCodeMaxLength")]
    [Display(Name = "ProProduct:InsurerProductCode")]
    public string? InsurerProductCode { get; set; }

    [Required(ErrorMessage = "ProProduct:ShortNameRequired")]
    [StringLength(50, ErrorMessage = "ProProduct:ShortNameMaxLength")]
    [Display(Name = "ProProduct:ShortName")]
    public string ShortName { get; set; } = null!;

    [Required(ErrorMessage = "ProProduct:NameRequired")]
    [StringLength(250, ErrorMessage = "ProProduct:NameMaxLength")]
    [Display(Name = "ProProduct:Name")]
    public string Name { get; set; } = null!;

    [StringLength(500, ErrorMessage = "ProProduct:DescriptionMaxLength")]
    [Display(Name = "ProProduct:Description")]
    public string? Description { get; set; }

    [StringLength(250, ErrorMessage = "ProProduct:InternalNoteMaxLength")]
    [Display(Name = "ProProduct:InternalNote")]
    public string? InternalNote { get; set; }

    [Required(ErrorMessage = "ProProduct:RateTypeRequired")]
    [StringLength(15, ErrorMessage = "ProProduct:RateTypeMaxLength")]
    [Display(Name = "ProProduct:RateType")]
    public string RateType { get; set; } = "table_rate";

    [Required(ErrorMessage = "ProProduct:StatusRequired")]
    [Display(Name = "ProProduct:Status")]
    public ProProductStatus Status { get; set; }

    [Display(Name = "ProProduct:SeqNumber")]
    [Range(0, int.MaxValue, ErrorMessage = "ProProduct:SeqNumberInvalid")]
    public int SeqNumber { get; set; } = 1;

    [Required(ErrorMessage = "ProProduct:EffectDateRequired")]
    [Display(Name = "ProProduct:EffectDate")]
    public DateTime EffectDate { get; set; }

    [Display(Name = "ProProduct:ExpireDate")]
    public DateTime? ExpireDate { get; set; }

    [Display(Name = "ProProduct:PlanDefinitionId")]
    public Guid? PlanDefinitionId { get; set; }

    [StringLength(1, ErrorMessage = "ProProduct:IsPlanMaxLength")]
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
