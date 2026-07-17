using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using iOne.Policies;

namespace iOne.Policy.Policies;

public class UpdatePolicyDetailDto
{
    // ⚠️ QUAN TRỌNG:
    // - Không cho phép update: PolicyNo / LastVersionId / Version (số version)

    /// <summary>
    /// Optional. When provided, the policy is linked to this contract (allows changing contract on edit).
    /// </summary>
    public Guid? ContractId { get; set; }

    public UpdatePolicyContractInputDto? Contract { get; set; }

    [Required(ErrorMessage = "Policy:LobIdRequired")]
    [Display(Name = "Policy:LobId")]
    public Guid LobId { get; set; }

    [Required(ErrorMessage = "Policy:SellTypeRequired")]
    [Display(Name = "Policy:SellType")]
    public PolicySellType SellType { get; set; }

    [StringLength(50, ErrorMessage = "Policy:InsurerPolicyNoMaxLength")]
    [Display(Name = "Policy:InsurerPolicyNo")]
    public string? InsurerPolicyNo { get; set; }

    [Required(ErrorMessage = "Policy:PolicyTypeIdRequired")]
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
    public PolicyStatus Status { get; set; }

    [Display(Name = "Policy:IssueDate")]
    public DateTime? IssueDate { get; set; }

    [StringLength(15, ErrorMessage = "Policy:ApprovalStatusMaxLength")]
    [Display(Name = "Policy:ApprovalStatus")]
    public string? ApprovalStatus { get; set; }

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

    [Display(Name = "Policy:Discount")]
    public decimal? Discount { get; set; }

    [Display(Name = "Policy:DiscountRate")]
    public decimal? DiscountRate { get; set; }

    [Required(ErrorMessage = "Policy:IsBankLoanRequired")]
    [StringLength(1, ErrorMessage = "Policy:IsBankLoanMaxLength")]
    [Display(Name = "Policy:IsBankLoan")]
    public string IsBankLoan { get; set; } = "N";

    [Display(Name = "Policy:ChannelId")]
    public Guid? ChannelId { get; set; }

    [StringLength(50, ErrorMessage = "Policy:LotImportCodeMaxLength")]
    [Display(Name = "Policy:LotImportCode")]
    public string? LotImportCode { get; set; }

    [StringLength(50, ErrorMessage = "Policy:CertificateNoMaxLength")]
    [Display(Name = "Policy:CertificateNo")]
    public string? CertificateNo { get; set; }

    // Insured info
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

    // Beneficiary info
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

    // Nested objects (same shape as create, but NOT reusing create DTO types)
    public UpdatePolicyVersionInputDto? Version { get; set; }

    public List<UpdatePolicyProductInputDto>? Products { get; set; }

    public UpdatePolicyRiskObjectInputDto? RiskObject { get; set; }

    public List<UpdatePolicyDocumentInputDto>? Documents { get; set; }

    public UpdatePolicyAmountInputDto? Amount { get; set; }

    /// <summary>
    /// Khi true: lưu và gửi duyệt (gọi Elsa workflow). Khi false: chỉ lưu nháp (không gọi Elsa).
    /// Default true để giữ tương thích.
    /// </summary>
    public bool SubmitForApproval { get; set; } = true;

    /// <summary>
    /// Khi sửa SĐBS update-in-place: chỉ định policy_version cần sửa.
    /// Nếu null thì dùng policy.LastVersionId (bản mới nhất).
    /// Cần truyền khi FE load theo versionId và user đang sửa version đó.
    /// </summary>
    public Guid? VersionId { get; set; }
}

