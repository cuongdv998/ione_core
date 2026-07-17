using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using iOne.Policies;
using iOne.PolicyContracts;

namespace iOne.Policy.Policies;

public class CreatePolicyDto
{
    public Guid? ContractId { get; set; }

    // Nested create payloads (optional) for creating related tables in a single request
    public CreatePolicyContractInputDto? Contract { get; set; }
    public CreatePolicyVersionInputDto? Version { get; set; }
    public CreatePolicyAmountInputDto? Amount { get; set; }
    public List<CreatePolicyProductInputDto>? Products { get; set; }
    public CreatePolicyRiskObjectInputDto? RiskObject { get; set; }
    public List<CreatePolicyDocumentInputDto>? Documents { get; set; }

    [Required(ErrorMessage = "Policy:LobIdRequired")]
    [Display(Name = "Policy:LobId")]
    public Guid LobId { get; set; }

    [StringLength(50, ErrorMessage = "Policy:PolicyNoMaxLength")]
    [Display(Name = "Policy:PolicyNo")]
    public string? PolicyNo { get; set; }

    [Required(ErrorMessage = "Policy:LastVersionIdRequired")]
    [Display(Name = "Policy:LastVersionId")]
    public Guid LastVersionId { get; set; }

    [Required(ErrorMessage = "Policy:SellTypeRequired")]
    [Display(Name = "Policy:SellType")]
    public PolicySellType SellType { get; set; }

    [StringLength(50, ErrorMessage = "Policy:InsurerPolicyNoMaxLength")]
    [Display(Name = "Policy:InsurerPolicyNo")]
    public string? InsurerPolicyNo { get; set; }

    [Display(Name = "Policy:PolicyTypeId")]
    public Guid PolicyTypeId { get; set; }

    [Required(ErrorMessage = "Policy:PartnerIdRequired")]
    [Display(Name = "Policy:PartnerId")]
    public Guid PartnerId { get; set; }

    [Required(ErrorMessage = "Policy:SellerIdRequired")]
    [Display(Name = "Policy:SellerId")]
    public Guid SellerId { get; set; }

    [Required(ErrorMessage = "Policy:ImplementerIdRequired")]
    [Display(Name = "Policy:ImplementerId")]
    public Guid ImplementerId { get; set; }

    [Required(ErrorMessage = "Policy:CurrencyIdRequired")]
    [Display(Name = "Policy:CurrencyId")]
    public Guid CurrencyId { get; set; }

    [Required(ErrorMessage = "Policy:ExchangeRateRequired")]
    [Display(Name = "Policy:ExchangeRate")]
    public decimal ExchangeRate { get; set; } = 1m;

    [Required(ErrorMessage = "Policy:StatusRequired")]
    [Display(Name = "Policy:Status")]
    public PolicyStatus Status { get; set; } = PolicyStatus.Draft;

    [Display(Name = "Policy:IssueDate")] public DateTime? IssueDate { get; set; }

    [StringLength(15, ErrorMessage = "Policy:ApprovalStatusMaxLength")]
    [Display(Name = "Policy:ApprovalStatus")]
    public string? ApprovalStatus { get; set; }

    [Display(Name = "Policy:CancellationDate")]
    public DateTime? CancellationDate { get; set; }

    [Display(Name = "Policy:TerminationDate")]
    public DateTime? TerminationDate { get; set; }

    [Display(Name = "Policy:CancellationReasonId")]
    public Guid? CancellationReasonId { get; set; }

    [Display(Name = "Policy:TerminationReasonId")]
    public Guid? TerminationReasonId { get; set; }

    [Required(ErrorMessage = "Policy:OrgEffectDateRequired")]
    [Display(Name = "Policy:OrgEffectDate")]
    public DateTime OrgEffectDate { get; set; }

    [Required(ErrorMessage = "Policy:OrgExpireDateRequired")]
    [Display(Name = "Policy:OrgExpireDate")]
    public DateTime OrgExpireDate { get; set; }

    [Required(ErrorMessage = "Policy:IsRenewalRequired")]
    [StringLength(1, ErrorMessage = "Policy:IsRenewalMaxLength")]
    [Display(Name = "Policy:IsRenewal")]
    public string IsRenewal { get; set; } = "N";

    [Required(ErrorMessage = "Policy:IsGiftRequired")]
    [StringLength(1, ErrorMessage = "Policy:IsGiftMaxLength")]
    [Display(Name = "Policy:IsGift")]
    public string IsGift { get; set; } = "N";

    [Required(ErrorMessage = "Policy:PremiumTotalRequired")]
    [Display(Name = "Policy:PremiumTotal")]
    public decimal PremiumTotal { get; set; }

    [Required(ErrorMessage = "Policy:PremiumRequired")]
    [Display(Name = "Policy:Premium")]
    public decimal Premium { get; set; }

    [Required(ErrorMessage = "Policy:VatRequired")]
    [Display(Name = "Policy:Vat")]
    public decimal Vat { get; set; }

    [Display(Name = "Policy:Discount")] public decimal? Discount { get; set; }

    [Display(Name = "Policy:DiscountRate")]
    public decimal? DiscountRate { get; set; }

    [Required(ErrorMessage = "Policy:IsBankLoanRequired")]
    [StringLength(1, ErrorMessage = "Policy:IsBankLoanMaxLength")]
    [Display(Name = "Policy:IsBankLoan")]
    public string IsBankLoan { get; set; } = "N";

    [Display(Name = "Policy:ChannelId")] public Guid? ChannelId { get; set; }

    [StringLength(50, ErrorMessage = "Policy:LotImportCodeMaxLength")]
    [Display(Name = "Policy:LotImportCode")]
    public string? LotImportCode { get; set; }

    [StringLength(50, ErrorMessage = "Policy:CertificateNoMaxLength")]
    [Display(Name = "Policy:CertificateNo")]
    public string? CertificateNo { get; set; }

    [StringLength(250, ErrorMessage = "Policy:InsuredNameMaxLength")]
    [Display(Name = "Policy:InsuredName")]
    public string? InsuredName { get; set; }

    [StringLength(15, ErrorMessage = "Policy:InsuredIdNoMaxLength")]
    [Display(Name = "Policy:InsuredIdNo")]
    public string? InsuredIdNo { get; set; }

    [StringLength(15, ErrorMessage = "Policy:InsuredTinMaxLength")]
    [Display(Name = "Policy:InsuredTin")]
    public string? InsuredTin { get; set; }

    [StringLength(25, ErrorMessage = "Policy:InsuredPassportMaxLength")]
    [Display(Name = "Policy:InsuredPassport")]
    public string? InsuredPassport { get; set; }

    [StringLength(15, ErrorMessage = "Policy:InsuredPhoneMaxLength")]
    [Display(Name = "Policy:InsuredPhone")]
    public string? InsuredPhone { get; set; }

    [StringLength(50, ErrorMessage = "Policy:InsuredEmailMaxLength")]
    [Display(Name = "Policy:InsuredEmail")]
    public string? InsuredEmail { get; set; }

    [Display(Name = "Policy:InsuredProvinceId")]
    public Guid? InsuredProvinceId { get; set; }

    [Display(Name = "Policy:InsuredWardId")]
    public Guid? InsuredWardId { get; set; }

    [StringLength(250, ErrorMessage = "Policy:InsuredAddressMaxLength")]
    [Display(Name = "Policy:InsuredAddress")]
    public string? InsuredAddress { get; set; }

    [StringLength(500, ErrorMessage = "Policy:InsuredFullAddressMaxLength")]
    [Display(Name = "Policy:InsuredFullAddress")]
    public string? InsuredFullAddress { get; set; }

    [StringLength(15, ErrorMessage = "Policy:InsuredOrgTypeMaxLength")]
    [Display(Name = "Policy:InsuredOrgType")]
    public string? InsuredOrgType { get; set; }

    [StringLength(250, ErrorMessage = "Policy:BeneficiaryNameMaxLength")]
    [Display(Name = "Policy:BeneficiaryName")]
    public string? BeneficiaryName { get; set; }

    [StringLength(15, ErrorMessage = "Policy:BeneficiaryIdNoMaxLength")]
    [Display(Name = "Policy:BeneficiaryIdNo")]
    public string? BeneficiaryIdNo { get; set; }

    [StringLength(15, ErrorMessage = "Policy:BeneficiaryTinMaxLength")]
    [Display(Name = "Policy:BeneficiaryTin")]
    public string? BeneficiaryTin { get; set; }

    [StringLength(25, ErrorMessage = "Policy:BeneficiaryPassportMaxLength")]
    [Display(Name = "Policy:BeneficiaryPassport")]
    public string? BeneficiaryPassport { get; set; }

    [StringLength(15, ErrorMessage = "Policy:BeneficiaryPhoneMaxLength")]
    [Display(Name = "Policy:BeneficiaryPhone")]
    public string? BeneficiaryPhone { get; set; }

    [StringLength(50, ErrorMessage = "Policy:BeneficiaryEmailMaxLength")]
    [Display(Name = "Policy:BeneficiaryEmail")]
    public string? BeneficiaryEmail { get; set; }

    [Display(Name = "Policy:BeneficiaryProvinceId")]
    public Guid? BeneficiaryProvinceId { get; set; }

    [Display(Name = "Policy:BeneficiaryWardId")]
    public Guid? BeneficiaryWardId { get; set; }

    [StringLength(250, ErrorMessage = "Policy:BeneficiaryAddressMaxLength")]
    [Display(Name = "Policy:BeneficiaryAddress")]
    public string? BeneficiaryAddress { get; set; }

    [StringLength(500, ErrorMessage = "Policy:BeneficiaryFullAddressMaxLength")]
    [Display(Name = "Policy:BeneficiaryFullAddress")]
    public string? BeneficiaryFullAddress { get; set; }

    [StringLength(15, ErrorMessage = "Policy:BeneficiaryOrgTypeMaxLength")]
    [Display(Name = "Policy:BeneficiaryOrgType")]
    public string? BeneficiaryOrgType { get; set; }
}

/// <summary>
///     Nested contract payload for Policy.CreateAsync.
///     NOTE: Intentionally omits Status/Cancellation/Termination/Quotation fields.
///     Server will set PolicyContract.Status to default.
/// </summary>
public class CreatePolicyContractInputDto
{
    public Guid? InsurerId { get; set; }

    [StringLength(50, ErrorMessage = "PolicyContract:InsurerContractCodeMaxLength")]
    [Display(Name = "PolicyContract:InsurerContractCode")]
    public string? InsurerContractCode { get; set; }

    [Display(Name = "PolicyContract:LobId")]
    public Guid? LobId { get; set; }

    [StringLength(50, ErrorMessage = "PolicyContract:CodeMaxLength")]
    [Display(Name = "PolicyContract:Code")]
    public string? Code { get; set; }

    [StringLength(250, ErrorMessage = "PolicyContract:NameMaxLength")]
    [Display(Name = "PolicyContract:Name")]
    public string? Name { get; set; }

    [Required(ErrorMessage = "PolicyContract:TypeRequired")]
    [Display(Name = "PolicyContract:Type")]
    public PolicyContractType Type { get; set; }

    [Required(ErrorMessage = "PolicyContract:CustomerIdRequired")]
    [Display(Name = "PolicyContract:CustomerId")]
    public Guid CustomerId { get; set; }

    [StringLength(50, ErrorMessage = "PolicyContract:PayerNameMaxLength")]
    [Display(Name = "PolicyContract:PayerName")]
    public string? PayerName { get; set; }

    [StringLength(50, ErrorMessage = "PolicyContract:PayerEmailMaxLength")]
    [Display(Name = "PolicyContract:PayerEmail")]
    public string? PayerEmail { get; set; }

    [StringLength(15, ErrorMessage = "PolicyContract:PayerPhoneMaxLength")]
    [Display(Name = "PolicyContract:PayerPhone")]
    public string? PayerPhone { get; set; }

    [Display(Name = "PolicyContract:PayerProvinceId")]
    public Guid? PayerProvinceId { get; set; }

    [Display(Name = "PolicyContract:PayerWardId")]
    public Guid? PayerWardId { get; set; }

    [StringLength(250, ErrorMessage = "PolicyContract:PayerAddressMaxLength")]
    [Display(Name = "PolicyContract:PayerAddress")]
    public string? PayerAddress { get; set; }

    [StringLength(250, ErrorMessage = "PolicyContract:PayerFullAddressMaxLength")]
    [Display(Name = "PolicyContract:PayerFullAddress")]
    public string? PayerFullAddress { get; set; }

    [StringLength(50, ErrorMessage = "PolicyContract:PayerTaxCodeMaxLength")]
    [Display(Name = "PolicyContract:PayerTaxCode")]
    public string? PayerTaxCode { get; set; }

    [StringLength(500, ErrorMessage = "PolicyContract:DescriptionMaxLength")]
    [Display(Name = "PolicyContract:Description")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "PolicyContract:EffectDateRequired")]
    [Display(Name = "PolicyContract:EffectDate")]
    public DateTime EffectDate { get; set; }

    [Display(Name = "PolicyContract:ExpireDate")]
    public DateTime? ExpireDate { get; set; }

    [Display(Name = "PolicyContract:Quantity")]
    public decimal? Quantity { get; set; }

    [Display(Name = "PolicyContract:CurrentQuantity")]
    public decimal? CurrentQuantity { get; set; }

    [Display(Name = "PolicyContract:EmployeeId")]
    public Guid? EmployeeId { get; set; }

    [Required(ErrorMessage = "PolicyContract:IsReciveInvoiceRequired")]
    [StringLength(1, ErrorMessage = "PolicyContract:IsReciveInvoiceMaxLength")]
    [Display(Name = "PolicyContract:IsReciveInvoice")]
    public string IsReciveInvoice { get; set; } = "N";

    public List<CreatePolicyContractDocumentInputDto>? Documents { get; set; }
}

public class CreatePolicyContractDocumentInputDto
{
    public Guid DocumentId { get; set; }
}

public class CreatePolicyVersionInputDto
{
    [Required(ErrorMessage = "PolicyVersion:VersionRequired")]
    [Display(Name = "PolicyVersion:Version")]
    public decimal Version { get; set; }

    [Required(ErrorMessage = "PolicyVersion:EffectDateRequired")]
    [Display(Name = "PolicyVersion:EffectDate")]
    public DateTime EffectDate { get; set; }

    [Required(ErrorMessage = "PolicyVersion:ExpireDateRequired")]
    [Display(Name = "PolicyVersion:ExpireDate")]
    public DateTime ExpireDate { get; set; }

    [Required(ErrorMessage = "PolicyVersion:OrgEffectDateRequired")]
    [Display(Name = "PolicyVersion:OrgEffectDate")]
    public DateTime OrgEffectDate { get; set; }

    [Required(ErrorMessage = "PolicyVersion:OrgExpireDateRequired")]
    [Display(Name = "PolicyVersion:OrgExpireDate")]
    public DateTime OrgExpireDate { get; set; }

    [StringLength(500, ErrorMessage = "PolicyVersion:InternalNoteMaxLength")]
    [Display(Name = "PolicyVersion:InternalNote")]
    public string? InternalNote { get; set; }

    [StringLength(500, ErrorMessage = "PolicyVersion:CustomerNoteMaxLength")]
    [Display(Name = "PolicyVersion:CustomerNote")]
    public string? CustomerNote { get; set; }

    [Required(ErrorMessage = "PolicyVersion:PremiumTotalRequired")]
    [Display(Name = "PolicyVersion:PremiumTotal")]
    public decimal PremiumTotal { get; set; }

    [Required(ErrorMessage = "PolicyVersion:PremiumRequired")]
    [Display(Name = "PolicyVersion:Premium")]
    public decimal Premium { get; set; }

    [Required(ErrorMessage = "PolicyVersion:VatRequired")]
    [Display(Name = "PolicyVersion:Vat")]
    public decimal Vat { get; set; }

    [Display(Name = "PolicyVersion:Discount")]
    public decimal? Discount { get; set; }

    [Display(Name = "PolicyVersion:DiscountRate")]
    public decimal? DiscountRate { get; set; }

    [Display(Name = "PolicyVersion:Markup")]
    public decimal? Markup { get; set; }
}

public class CreatePolicyAmountInputDto
{
    [Display(Name = "PolicyAmount:FeeItemId")]
    public Guid? FeeItemId { get; set; }

    [Display(Name = "Policy:PremiumTotal")]
    public decimal PremiumTotal { get; set; }

    [Display(Name = "Policy:Premium")] public decimal Premium { get; set; }

    [Display(Name = "Policy:Vat")] public decimal Vat { get; set; }

    [Display(Name = "Policy:Discount")] public decimal? Discount { get; set; }

    [Display(Name = "Policy:DiscountRate")]
    public decimal? DiscountRate { get; set; }

    [Display(Name = "Policy:Markup")]
    public decimal? Markup { get; set; }
}

public class CreatePolicyProductInputDto
{
    [Required(ErrorMessage = "PolicyProduct:ProductIdRequired")]
    [Display(Name = "PolicyProduct:ProductId")]
    public Guid ProductId { get; set; }

    [StringLength(50, ErrorMessage = "PolicyProduct:InsurerProductCodeMaxLength")]
    [Display(Name = "PolicyProduct:InsurerProductCode")]
    public string? InsurerProductCode { get; set; }

    [Display(Name = "PolicyProduct:AmountLiability")]
    public decimal? AmountLiability { get; set; }

    [Required(ErrorMessage = "PolicyProduct:PremiumTotalRequired")]
    [Display(Name = "PolicyProduct:PremiumTotal")]
    public decimal PremiumTotal { get; set; }

    [Required(ErrorMessage = "PolicyProduct:PremiumRequired")]
    [Display(Name = "PolicyProduct:Premium")]
    public decimal Premium { get; set; }

    [Required(ErrorMessage = "PolicyProduct:VatRequired")]
    [Display(Name = "PolicyProduct:Vat")]
    public decimal Vat { get; set; }

    [Display(Name = "PolicyProduct:Discount")]
    public decimal? Discount { get; set; }

    [Display(Name = "PolicyProduct:DiscountRate")]
    public decimal? DiscountRate { get; set; }

    [Display(Name = "PolicyProduct:Markup")]
    public decimal? Markup { get; set; }

    // Product attributes with isRequired from by-lob-partner API (for validation: isRequired=Y => value must be present)
    public List<PolicyProductAttributeInputDto>? Attributes { get; set; }

    // Nested coverages for this product (policy_coverage + policy_coverage_level)
    public List<CreatePolicyCoverageInputDto>? Coverages { get; set; }
}

/// <summary>
/// Attribute value + isRequired from product's ProductAttributes (by-lob-partner).
/// Backend validates: if IsRequired = "Y", Value must be non-empty.
/// </summary>
public class PolicyProductAttributeInputDto
{
    public string Code { get; set; } = string.Empty;

    [JsonConverter(typeof(PolicyProductAttributeValueJsonConverter))]
    public string? Value { get; set; }
    public string IsRequired { get; set; } = "N";
    public decimal? AmountLiability { get; set; }

    public string? AttributeName { get; set; }
}

public class CreatePolicyCoverageInputDto
{
    [Required(ErrorMessage = "PolicyCoverage:CoverageIdRequired")]
    [Display(Name = "PolicyCoverage:CoverageId")]
    public Guid CoverageId { get; set; }

    [Display(Name = "PolicyCoverage:CoverageParentId")]
    public Guid? CoverageParentId { get; set; }

    [StringLength(50, ErrorMessage = "PolicyCoverage:InsurerCoverageCodeMaxLength")]
    [Display(Name = "PolicyCoverage:InsurerCoverageCode")]
    public string? InsurerCoverageCode { get; set; }

    [Required(ErrorMessage = "PolicyCoverage:UomIdRequired")]
    [Display(Name = "PolicyCoverage:UomId")]
    public Guid UomId { get; set; }

    [Display(Name = "PolicyCoverage:TableRateLineId")]
    public Guid? TableRateLineId { get; set; }

    [Display(Name = "PolicyCoverage:AmountLiability")]
    public decimal? AmountLiability { get; set; }

    [Required(ErrorMessage = "PolicyCoverage:QuantityRequired")]
    [Display(Name = "PolicyCoverage:Quantity")]
    public decimal Quantity { get; set; }

    [Required(ErrorMessage = "PolicyCoverage:TaxIdRequired")]
    [Display(Name = "PolicyCoverage:TaxId")]
    public Guid TaxId { get; set; }

    [Display(Name = "PolicyCoverage:NetRate")]
    public decimal? NetRate { get; set; }

    [Display(Name = "PolicyCoverage:BaseRate")]
    public decimal? BaseRate { get; set; }

    [Display(Name = "PolicyCoverage:FlatRate")]
    public decimal? FlatRate { get; set; }

    [Display(Name = "PolicyCoverage:Loading")]
    public decimal? Loading { get; set; }

    [Required(ErrorMessage = "PolicyCoverage:PremiumRateRequired")]
    [Display(Name = "PolicyCoverage:PremiumRate")]
    public decimal PremiumRate { get; set; }

    [Required(ErrorMessage = "PolicyCoverage:PremiumTotalRequired")]
    [Display(Name = "PolicyCoverage:PremiumTotal")]
    public decimal PremiumTotal { get; set; }

    [Required(ErrorMessage = "PolicyCoverage:PremiumRequired")]
    [Display(Name = "PolicyCoverage:Premium")]
    public decimal Premium { get; set; }

    [Required(ErrorMessage = "PolicyCoverage:VatRequired")]
    [Display(Name = "PolicyCoverage:Vat")]
    public decimal Vat { get; set; }

    [Display(Name = "PolicyCoverage:Discount")]
    public decimal? Discount { get; set; }

    [Display(Name = "PolicyCoverage:DiscountRate")]
    public decimal? DiscountRate { get; set; }

    public List<CreatePolicyCoverageLevelInputDto>? CoverageLevels { get; set; }
}

public class CreatePolicyCoverageLevelInputDto
{
    [Required(ErrorMessage = "PolicyCoverageLevel:CoverageLevelTypeIdRequired")]
    [Display(Name = "PolicyCoverageLevel:CoverageLevelTypeId")]
    public Guid CoverageLevelTypeId { get; set; }

    [Required(ErrorMessage = "PolicyCoverageLevel:CoverageLevelBasisIdRequired")]
    [Display(Name = "PolicyCoverageLevel:CoverageLevelBasisId")]
    public Guid CoverageLevelBasisId { get; set; }

    [Display(Name = "PolicyCoverageLevel:ConditionScript")]
    public string? ConditionScript { get; set; }

    [Display(Name = "PolicyCoverageLevel:ComputeScript")]
    public string? ComputeScript { get; set; }

    [Required(ErrorMessage = "PolicyCoverageLevel:AmountTypeRequired")]
    [StringLength(15, ErrorMessage = "PolicyCoverageLevel:AmountTypeMaxLength")]
    [Display(Name = "PolicyCoverageLevel:AmountType")]
    public string AmountType { get; set; } = null!;

    [Required(ErrorMessage = "PolicyCoverageLevel:FromAmountRequired")]
    [Display(Name = "PolicyCoverageLevel:FromAmount")]
    public decimal FromAmount { get; set; }

    [Required(ErrorMessage = "PolicyCoverageLevel:ToAmountRequired")]
    [Display(Name = "PolicyCoverageLevel:ToAmount")]
    public decimal ToAmount { get; set; }
}

public class CreatePolicyRiskObjectInputDto
{
    [Required(ErrorMessage = "PolicyRiskObject:ObjectTypeIdRequired")]
    [Display(Name = "PolicyRiskObject:ObjectTypeId")]
    public Guid ObjectTypeId { get; set; }

    [StringLength(250, ErrorMessage = "PolicyRiskObject:RepNameMaxLength")]
    [Display(Name = "PolicyRiskObject:RepName")]
    public string? RepName { get; set; }

    [StringLength(15, ErrorMessage = "PolicyRiskObject:RepIdNoMaxLength")]
    [Display(Name = "PolicyRiskObject:RepIdNo")]
    public string? RepIdNo { get; set; }

    [StringLength(15, ErrorMessage = "PolicyRiskObject:RepPassportMaxLength")]
    [Display(Name = "PolicyRiskObject:RepPassport")]
    public string? RepPassport { get; set; }

    [StringLength(15, ErrorMessage = "PolicyRiskObject:RepPhoneMaxLength")]
    [Display(Name = "PolicyRiskObject:RepPhone")]
    public string? RepPhone { get; set; }

    [StringLength(50, ErrorMessage = "PolicyRiskObject:RepEmailMaxLength")]
    [Display(Name = "PolicyRiskObject:RepEmail")]
    public string? RepEmail { get; set; }

    [Display(Name = "PolicyRiskObject:RepProvinceId")]
    public Guid? RepProvinceId { get; set; }

    [Display(Name = "PolicyRiskObject:RepWardId")]
    public Guid? RepWardId { get; set; }

    [StringLength(250, ErrorMessage = "PolicyRiskObject:RepAddressMaxLength")]
    [Display(Name = "PolicyRiskObject:RepAddress")]
    public string? RepAddress { get; set; }

    [StringLength(250, ErrorMessage = "PolicyRiskObject:RepFullAddressMaxLength")]
    [Display(Name = "PolicyRiskObject:RepFullAddress")]
    public string? RepFullAddress { get; set; }

    [Display(Name = "PolicyRiskObject:RiskObjectProvinceId")]
    public Guid? RiskObjectProvinceId { get; set; }

    [Display(Name = "PolicyRiskObject:RiskObjectWardId")]
    public Guid? RiskObjectWardId { get; set; }

    [StringLength(250, ErrorMessage = "PolicyRiskObject:RiskObjectAddressMaxLength")]
    [Display(Name = "PolicyRiskObject:RiskObjectAddress")]
    public string? RiskObjectAddress { get; set; }

    [StringLength(250, ErrorMessage = "PolicyRiskObject:RiskObjectFullAddressMaxLength")]
    [Display(Name = "PolicyRiskObject:RiskObjectFullAddress")]
    public string? RiskObjectFullAddress { get; set; }

    [Display(Name = "PolicyRiskObject:RiskObjectLat")]
    public double? RiskObjectLat { get; set; }

    [Display(Name = "PolicyRiskObject:RiskObjectLong")]
    public double? RiskObjectLong { get; set; }

    /// <summary>
    ///     Documents linked to this risk object (stored in policy_risk_object_document).
    /// </summary>
    public List<CreatePolicyRiskObjectDocumentInputDto>? Documents { get; set; }

    // Nested motor detail (for Motor risk object)
    public CreatePolicyRiskMotorInputDto? RiskObjectMotor { get; set; }
}

public class CreatePolicyRiskObjectDocumentInputDto
{
    public Guid? DocumentId { get; set; }
}

/// <summary>
///     Nested risk motor payload for Policy.CreateAsync.
///     NOTE: Intentionally omits PolicyRiskObjectId; server links it automatically.
/// </summary>
public class CreatePolicyRiskMotorInputDto
{
    [Display(Name = "PolicyRiskMotor:RiskObjectValue")]
    public decimal? RiskObjectValue { get; set; }

    [StringLength(50, ErrorMessage = "PolicyRiskMotor:MotorClassCodeMaxLength")]
    [Display(Name = "PolicyRiskMotor:MotorClassCode")]
    public string? MotorClassCode { get; set; }

    [StringLength(50, ErrorMessage = "PolicyRiskMotor:CarLineCodeMaxLength")]
    [Display(Name = "PolicyRiskMotor:CarLineCode")]
    public string? CarLineCode { get; set; }

    [StringLength(50, ErrorMessage = "PolicyRiskMotor:CarGroupCodeMaxLength")]
    [Display(Name = "PolicyRiskMotor:CarGroupCode")]
    public string? CarGroupCode { get; set; }

    [StringLength(50, ErrorMessage = "PolicyRiskMotor:CarTypeCodeMaxLength")]
    [Display(Name = "PolicyRiskMotor:CarTypeCode")]
    public string? CarTypeCode { get; set; }

    [StringLength(50, ErrorMessage = "PolicyRiskMotor:CarBrandCodeMaxLength")]
    [Display(Name = "PolicyRiskMotor:CarBrandCode")]
    public string? CarBrandCode { get; set; }

    [StringLength(50, ErrorMessage = "PolicyRiskMotor:CarModelCodeMaxLength")]
    [Display(Name = "PolicyRiskMotor:CarModelCode")]
    public string? CarModelCode { get; set; }

    [StringLength(50, ErrorMessage = "PolicyRiskMotor:CarCategoryCodeMaxLength")]
    [Display(Name = "PolicyRiskMotor:CarCategoryCode")]
    public string? CarCategoryCode { get; set; }

    [StringLength(15, ErrorMessage = "PolicyRiskMotor:CarUsageMaxLength")]
    [Display(Name = "PolicyRiskMotor:CarUsage")]
    public string? CarUsage { get; set; }

    [Display(Name = "PolicyRiskMotor:CarOld")]
    public decimal? CarOld { get; set; }

    [Display(Name = "PolicyRiskMotor:CarProductionYear")]
    public DateTime? CarProductionYear { get; set; }

    [StringLength(15, ErrorMessage = "PolicyRiskMotor:CarPlateMaxLength")]
    [Display(Name = "PolicyRiskMotor:CarPlate")]
    public string? CarPlate { get; set; }

    [StringLength(50, ErrorMessage = "PolicyRiskMotor:CarPlateTypeMaxLength")]
    [Display(Name = "PolicyRiskMotor:CarPlateType")]
    public string? CarPlateType { get; set; }

    [StringLength(15, ErrorMessage = "PolicyRiskMotor:CarPlateClearMaxLength")]
    [Display(Name = "PolicyRiskMotor:CarPlateClear")]
    public string? CarPlateClear { get; set; }

    [Display(Name = "PolicyRiskMotor:CarSeatNumber")]
    public decimal? CarSeatNumber { get; set; }

    [StringLength(25, ErrorMessage = "PolicyRiskMotor:CarVinMaxLength")]
    [Display(Name = "PolicyRiskMotor:CarVin")]
    public string? CarVin { get; set; }

    [StringLength(25, ErrorMessage = "PolicyRiskMotor:CarEngineNumberMaxLength")]
    [Display(Name = "PolicyRiskMotor:CarEngineNumber")]
    public string? CarEngineNumber { get; set; }

    [Display(Name = "PolicyRiskMotor:CarPayloadCapacity")]
    public decimal? CarPayloadCapacity { get; set; }

    [StringLength(15, ErrorMessage = "PolicyRiskMotor:CarColorMaxLength")]
    [Display(Name = "PolicyRiskMotor:CarColor")]
    public string? CarColor { get; set; }

    [StringLength(50, ErrorMessage = "PolicyRiskMotor:CarOriginMaxLength")]
    [Display(Name = "PolicyRiskMotor:CarOrigin")]
    public string? CarOrigin { get; set; }

    [StringLength(1, ErrorMessage = "PolicyRiskMotor:CarNewMaxLength")]
    [Display(Name = "PolicyRiskMotor:CarNew")]
    public string? CarNew { get; set; }
}

public class CreatePolicyDocumentInputDto
{
    [Display(Name = "PolicyDocument:DocumentId")]
    public Guid? DocumentId { get; set; }
}