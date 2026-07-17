using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using iOne.Policies;
using Volo.Abp.Application.Dtos;

namespace iOne.Policy.Policies;

public class PolicyDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "Policy:ContractId")]
    public Guid? ContractId { get; set; }

    [Display(Name = "Policy:LobId")]
    public Guid LobId { get; set; }

    [Display(Name = "Policy:PolicyNo")]
    public string PolicyNo { get; set; } = null!;

    [Display(Name = "Policy:LastVersionId")]
    public Guid LastVersionId { get; set; }

    [Display(Name = "Policy:SellType")]
    public PolicySellType SellType { get; set; }

    [Display(Name = "Policy:InsurerPolicyNo")]
    public string? InsurerPolicyNo { get; set; }

    [Display(Name = "Policy:PolicyTypeId")]
    public Guid PolicyTypeId { get; set; }

    [Display(Name = "Policy:PartnerId")]
    public Guid PartnerId { get; set; }

    [Display(Name = "Policy:SellerId")]
    public Guid SellerId { get; set; }

    [Display(Name = "Policy:ImplementerId")]
    public Guid ImplementerId { get; set; }

    [Display(Name = "Policy:CurrencyId")]
    public Guid CurrencyId { get; set; }

    [Display(Name = "Policy:ExchangeRate")]
    public decimal ExchangeRate { get; set; }

    [Display(Name = "Policy:Status")]
    public PolicyStatus Status { get; set; }

    [Display(Name = "Policy:IssueDate")]
    public DateTime? IssueDate { get; set; }

    [Display(Name = "Policy:ApprovalStatus")]
    public string? ApprovalStatus { get; set; }

    [Display(Name = "Policy:TerminationStatus")]
    public PolicyTerminationStatus? TerminationStatus { get; set; }

    [Display(Name = "Policy:CancellationDate")]
    public DateTime? CancellationDate { get; set; }

    [Display(Name = "Policy:TerminationDate")]
    public DateTime? TerminationDate { get; set; }

    [Display(Name = "Policy:CancellationReasonId")]
    public Guid? CancellationReasonId { get; set; }

    [Display(Name = "Policy:TerminationReasonId")]
    public Guid? TerminationReasonId { get; set; }

    [Display(Name = "Policy:TerminationReasonDescription")]
    public string? TerminationReasonDescription { get; set; }

    [Display(Name = "Policy:OrgEffectDate")]
    public DateTime OrgEffectDate { get; set; }

    [Display(Name = "Policy:OrgExpireDate")]
    public DateTime OrgExpireDate { get; set; }

    [Display(Name = "Policy:IsRenewal")]
    public string IsRenewal { get; set; } = "N";

    [Display(Name = "Policy:IsGift")]
    public string IsGift { get; set; } = "N";

    [Display(Name = "Policy:PremiumTotal")]
    public decimal PremiumTotal { get; set; }

    [Display(Name = "Policy:Premium")]
    public decimal Premium { get; set; }

    [Display(Name = "Policy:Vat")]
    public decimal Vat { get; set; }

    [Display(Name = "Policy:Discount")]
    public decimal? Discount { get; set; }

    [Display(Name = "Policy:DiscountRate")]
    public decimal? DiscountRate { get; set; }

    [Display(Name = "Policy:IsBankLoan")]
    public string IsBankLoan { get; set; } = "N";

    [Display(Name = "Policy:ChannelId")]
    public Guid? ChannelId { get; set; }

    [Display(Name = "Policy:LotImportCode")]
    public string? LotImportCode { get; set; }

    [Display(Name = "Policy:InsuredName")]
    public string? InsuredName { get; set; }

    [Display(Name = "Policy:InsuredIdNo")]
    public string? InsuredIdNo { get; set; }

    [Display(Name = "Policy:InsuredTin")]
    public string? InsuredTin { get; set; }

    [Display(Name = "Policy:InsuredPassport")]
    public string? InsuredPassport { get; set; }

    [Display(Name = "Policy:InsuredPhone")]
    public string? InsuredPhone { get; set; }

    [Display(Name = "Policy:InsuredEmail")]
    public string? InsuredEmail { get; set; }

    [Display(Name = "Policy:InsuredProvinceId")]
    public Guid? InsuredProvinceId { get; set; }

    [Display(Name = "Policy:InsuredWardId")]
    public Guid? InsuredWardId { get; set; }

    [Display(Name = "Policy:InsuredAddress")]
    public string? InsuredAddress { get; set; }

    [Display(Name = "Policy:InsuredFullAddress")]
    public string? InsuredFullAddress { get; set; }

    [Display(Name = "Policy:InsuredOrgType")]
    public string? InsuredOrgType { get; set; }

    [Display(Name = "Policy:BeneficiaryName")]
    public string? BeneficiaryName { get; set; }

    [Display(Name = "Policy:BeneficiaryIdNo")]
    public string? BeneficiaryIdNo { get; set; }

    [Display(Name = "Policy:BeneficiaryTin")]
    public string? BeneficiaryTin { get; set; }

    [Display(Name = "Policy:BeneficiaryPassport")]
    public string? BeneficiaryPassport { get; set; }

    [Display(Name = "Policy:BeneficiaryPhone")]
    public string? BeneficiaryPhone { get; set; }

    [Display(Name = "Policy:BeneficiaryEmail")]
    public string? BeneficiaryEmail { get; set; }

    [Display(Name = "Policy:BeneficiaryProvinceId")]
    public Guid? BeneficiaryProvinceId { get; set; }

    [Display(Name = "Policy:BeneficiaryWardId")]
    public Guid? BeneficiaryWardId { get; set; }

    [Display(Name = "Policy:BeneficiaryAddress")]
    public string? BeneficiaryAddress { get; set; }

    [Display(Name = "Policy:BeneficiaryFullAddress")]
    public string? BeneficiaryFullAddress { get; set; }

    [Display(Name = "Policy:BeneficiaryOrgType")]
    public string? BeneficiaryOrgType { get; set; }

    // New properties for wireframe implementation - Related entity names/data
    [Display(Name = "Policy:ContractNo")]
    public string? ContractNo { get; set; }

    [Display(Name = "Policy:CertificateNo")]
    public string? CertificateNo { get; set; }

    [Display(Name = "Policy:ProductName")]
    public string? ProductName { get; set; }

    [Display(Name = "Policy:RootInsuranceName")]
    public string? RootInsuranceName { get; set; }

    [Display(Name = "Policy:CustomerName")]
    public string? CustomerName { get; set; }

    [Display(Name = "Policy:CustomerId")]
    public Guid? CustomerId { get; set; }

    // Vehicle Info (for car insurance)
    [Display(Name = "Policy:VehiclePlate")]
    public string? VehiclePlate { get; set; }

    [Display(Name = "Policy:ChassisNumber")]
    public string? ChassisNumber { get; set; }

    [Display(Name = "Policy:EngineNumber")]
    public string? EngineNumber { get; set; }

    // Channel & Partner Info
    [Display(Name = "Policy:ChannelName")]
    public string? ChannelName { get; set; }

    [Display(Name = "Policy:PartnerName")]
    public string? PartnerName { get; set; }

    // Personnel Info
    [Display(Name = "Policy:ImplementerName")]
    public string? ImplementerName { get; set; }

    [Display(Name = "Policy:PolicyIssuerName")]
    public string? PolicyIssuerName { get; set; }

    [Display(Name = "Policy:PolicyIssuerId")]
    public Guid? PolicyIssuerId { get; set; }

    // Version & Root Policy
    [Display(Name = "Policy:Version")]
    public int Version { get; set; }

    [Display(Name = "Policy:IsRootPolicy")]
    public bool IsRootPolicy { get; set; }

    /// <summary>
    /// True nếu row này là version lớn nhất (policy.LastVersionId = version này). Dùng để chỉ hiện nút SĐBS trên đơn version mới nhất.
    /// </summary>
    public bool IsLatestVersion { get; set; }

    // Status Info
    [Display(Name = "Policy:PaymentStatus")]
    public string? PaymentStatus { get; set; }

    [Display(Name = "Policy:PaymentId")]
    public Guid? PaymentId { get; set; }

    [Display(Name = "Policy:PaymentProvider")]
    public string? PaymentProvider { get; set; }

    [Display(Name = "Policy:ApprovalDate")]
    public DateTime? ApprovalDate { get; set; }

    [Display(Name = "Policy:ApproverName")]
    public string? ApproverName { get; set; }

    [Display(Name = "Policy:ApproverId")]
    public Guid? ApproverId { get; set; }

    // Creator Info
    [Display(Name = "Policy:CreatorName")]
    public string? CreatorName { get; set; }

    /// <summary>
    /// When policy is in pending approval, the assignee id of the latest work_task (WaitApprove) for this policy; otherwise null.
    /// Used by list to show Approve action only when current user is the assignee.
    /// </summary>
    public Guid? CurrentWorkTaskAssigneeId { get; set; }

    /// <summary>
    /// When policy is in pending approval, the id of the latest work_task (WaitApprove) for this policy; otherwise null.
    /// Same work task as CurrentWorkTaskAssigneeId. Used by list to open approval detail (e.g. in new tab).
    /// </summary>
    public Guid? CurrentWorkTaskId { get; set; }

    // Nested objects for detail view/edit (dedicated DTOs for GetAsync)
    public PolicyContractDetailDto? Contract { get; set; }
    public PolicyVersionDetailDto? VersionDetail { get; set; }

    /// <summary>
    /// All versions of this policy (for list API). Ordered by Version ascending.
    /// </summary>
    public List<PolicyVersionListItemDto>? Versions { get; set; }
    public PolicyAmountDetailDto? Amount { get; set; }
    public List<PolicyProductDetailDto>? Products { get; set; }
    public PolicyRiskObjectDetailDto? RiskObject { get; set; }
    public List<PolicyDocumentDetailDto>? Documents { get; set; }

}
