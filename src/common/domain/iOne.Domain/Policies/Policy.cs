using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using iOne.HrEmployees;
using iOne.PolicyContracts;
using iOne.PolicyTypes;
using iOne.ProLineOfBusinesses;
using iOne.ResCurrencies;
using iOne.ResPartners;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;

namespace iOne.Policies;

[Table("policy")]
public class Policy : FullAuditedAggregateRoot<Guid>, IEntity<Guid>
{
    // Basic Info
    public virtual Guid? ContractId { get; private set; }

    [Required]
    public virtual Guid LobId { get; private set; }

    [Required]
    [MaxLength(50)]
    public virtual string PolicyNo { get; private set; } = null!;

    [Required]
    public virtual Guid LastVersionId { get; private set; }

    [Required]
    public virtual PolicySellType SellType { get; private set; }

    [MaxLength(50)]
    public virtual string? InsurerPolicyNo { get; private set; }

    [Required]
    public virtual Guid PolicyTypeId { get; private set; }

    [Required]
    public virtual Guid PartnerId { get; private set; }

    [Required]
    public virtual Guid SellerId { get; private set; }

    [Required]
    public virtual Guid ImplementerId { get; private set; }

    [Required]
    public virtual Guid CurrencyId { get; private set; }

    [Required]
    [Column(TypeName = "NUMERIC(15,3)")]
    public virtual decimal ExchangeRate { get; private set; }

    [Required]
    public virtual PolicyStatus Status { get; private set; }

    // Dates
    public virtual DateTime? IssueDate { get; private set; }

    [MaxLength(15)]
    public virtual string? ApprovalStatus { get; private set; }

    public virtual DateTime? CancellationDate { get; private set; }

    public virtual DateTime? TerminationDate { get; private set; }

    public virtual Guid? CancellationReasonId { get; private set; }

    public virtual Guid? TerminationReasonId { get; private set; }
    
    [MaxLength(500)]
    public virtual string? TerminationReasonDescription { get; private set; }

    [Required]
    public virtual DateTime OrgEffectDate { get; private set; }

    [Required]
    public virtual DateTime OrgExpireDate { get; private set; }

    // Flags
    [Required]
    [MaxLength(1)]
    public virtual string IsRenewal { get; private set; } = "N";

    [Required]
    [MaxLength(1)]
    public virtual string IsGift { get; private set; } = "N";

    // Financial
    [Required]
    [Column(TypeName = "NUMERIC(15,3)")]
    public virtual decimal PremiumTotal { get; private set; }

    [Required]
    [Column(TypeName = "NUMERIC(15,3)")]
    public virtual decimal Premium { get; private set; }

    [Required]
    [Column(TypeName = "NUMERIC(15,3)")]
    public virtual decimal Vat { get; private set; }

    [Column(TypeName = "NUMERIC(15,3)")]
    public virtual decimal? Discount { get; private set; }

    [Column(TypeName = "NUMERIC(15,3)")]
    public virtual decimal? DiscountRate { get; private set; }

    [Required]
    [MaxLength(1)]
    public virtual string IsBankLoan { get; private set; } = "N";

    // Channel & Import
    public virtual Guid? ChannelId { get; private set; }

    [MaxLength(50)]
    public virtual string? LotImportCode { get; private set; }

    // Insured Info
    [MaxLength(250)]
    public virtual string? InsuredName { get; private set; }

    [MaxLength(15)]
    public virtual string? InsuredIdNo { get; private set; }

    [MaxLength(15)]
    public virtual string? InsuredTin { get; private set; }

    [MaxLength(25)]
    public virtual string? InsuredPassport { get; private set; }

    [MaxLength(15)]
    public virtual string? InsuredPhone { get; private set; }

    [MaxLength(50)]
    public virtual string? InsuredEmail { get; private set; }

    public virtual Guid? InsuredProvinceId { get; private set; }

    public virtual Guid? InsuredWardId { get; private set; }

    [MaxLength(250)]
    public virtual string? InsuredAddress { get; private set; }

    [MaxLength(500)]
    public virtual string? InsuredFullAddress { get; private set; }

    [MaxLength(15)]
    public virtual string? InsuredOrgType { get; private set; }

    // Beneficiary Info
    [MaxLength(250)]
    public virtual string? BeneficiaryName { get; private set; }

    [MaxLength(15)]
    public virtual string? BeneficiaryIdNo { get; private set; }

    [MaxLength(15)]
    public virtual string? BeneficiaryTin { get; private set; }

    [MaxLength(25)]
    public virtual string? BeneficiaryPassport { get; private set; }

    [MaxLength(15)]
    public virtual string? BeneficiaryPhone { get; private set; }

    [MaxLength(50)]
    public virtual string? BeneficiaryEmail { get; private set; }

    public virtual Guid? BeneficiaryProvinceId { get; private set; }

    public virtual Guid? BeneficiaryWardId { get; private set; }

    [MaxLength(250)]
    public virtual string? BeneficiaryAddress { get; private set; }

    [MaxLength(500)]
    public virtual string? BeneficiaryFullAddress { get; private set; }

    [MaxLength(15)]
    public virtual string? BeneficiaryOrgType { get; private set; }

    // Navigation Properties
    public virtual PolicyContract? Contract { get; set; }
    public virtual ProLineOfBusiness Lob { get; set; } = null!;
    public virtual PolicyType PolicyType { get; set; } = null!;
    public virtual ResPartner Partner { get; set; } = null!;
    public virtual HrEmployee Seller { get; set; } = null!;
    public virtual HrEmployee Implementer { get; set; } = null!;
    public virtual ResCurrency Currency { get; set; } = null!;
    public virtual ICollection<PolicyVersion> PolicyVersions { get; set; } = new List<PolicyVersion>();
    public virtual ICollection<PolicyCertificate> PolicyCertificates { get; set; } = new List<PolicyCertificate>();
    public virtual ICollection<PolicyRiskObject> PolicyRiskObjects { get; set; } = new List<PolicyRiskObject>();

    protected Policy()
    {
        // For ORM
    }

    public Policy(
        Guid id,
        Guid lobId,
        string policyNo,
        Guid lastVersionId,
        PolicySellType sellType,
        Guid policyTypeId,
        Guid partnerId,
        Guid sellerId,
        Guid implementerId,
        Guid currencyId,
        decimal exchangeRate,
        PolicyStatus status,
        DateTime orgEffectDate,
        DateTime orgExpireDate,
        decimal premiumTotal,
        decimal premium,
        decimal vat,
        Guid? contractId = null,
        string? insurerPolicyNo = null,
        DateTime? issueDate = null,
        string? approvalStatus = null,
        DateTime? cancellationDate = null,
        DateTime? terminationDate = null,
        Guid? cancellationReasonId = null,
        Guid? terminationReasonId = null,
        string isRenewal = "N",
        string isGift = "N",
        decimal? discount = null,
        decimal? discountRate = null,
        string isBankLoan = "N",
        Guid? channelId = null,
        string? lotImportCode = null,
        string? insuredName = null,
        string? insuredIdNo = null,
        string? insuredTin = null,
        string? insuredPassport = null,
        string? insuredPhone = null,
        string? insuredEmail = null,
        Guid? insuredProvinceId = null,
        Guid? insuredWardId = null,
        string? insuredAddress = null,
        string? insuredFullAddress = null,
        string? insuredOrgType = null,
        string? beneficiaryName = null,
        string? beneficiaryIdNo = null,
        string? beneficiaryTin = null,
        string? beneficiaryPassport = null,
        string? beneficiaryPhone = null,
        string? beneficiaryEmail = null,
        Guid? beneficiaryProvinceId = null,
        Guid? beneficiaryWardId = null,
        string? beneficiaryAddress = null,
        string? beneficiaryFullAddress = null,
        string? beneficiaryOrgType = null)
        : base(id)
    {
        SetContractId(contractId);
        SetLobId(lobId);
        SetPolicyNo(policyNo);
        SetLastVersionId(lastVersionId);
        SetSellType(sellType);
        SetInsurerPolicyNo(insurerPolicyNo);
        SetPolicyTypeId(policyTypeId);
        SetPartnerId(partnerId);
        SetSellerId(sellerId);
        SetImplementerId(implementerId);
        SetCurrencyId(currencyId);
        SetExchangeRate(exchangeRate);
        SetStatus(status);
        SetIssueDate(issueDate);
        SetApprovalStatus(approvalStatus);
        SetCancellationDate(cancellationDate);
        SetTerminationDate(terminationDate);
        SetCancellationReasonId(cancellationReasonId);
        SetTerminationReasonId(terminationReasonId);
        SetOrgEffectDate(orgEffectDate);
        SetOrgExpireDate(orgExpireDate);
        SetIsRenewal(isRenewal);
        SetIsGift(isGift);
        SetPremiumTotal(premiumTotal);
        SetPremium(premium);
        SetVat(vat);
        SetDiscount(discount);
        SetDiscountRate(discountRate);
        SetIsBankLoan(isBankLoan);
        SetChannelId(channelId);
        SetLotImportCode(lotImportCode);
        SetInsuredName(insuredName);
        SetInsuredIdNo(insuredIdNo);
        SetInsuredTin(insuredTin);
        SetInsuredPassport(insuredPassport);
        SetInsuredPhone(insuredPhone);
        SetInsuredEmail(insuredEmail);
        SetInsuredProvinceId(insuredProvinceId);
        SetInsuredWardId(insuredWardId);
        SetInsuredAddress(insuredAddress);
        SetInsuredFullAddress(insuredFullAddress);
        SetInsuredOrgType(insuredOrgType);
        SetBeneficiaryName(beneficiaryName);
        SetBeneficiaryIdNo(beneficiaryIdNo);
        SetBeneficiaryTin(beneficiaryTin);
        SetBeneficiaryPassport(beneficiaryPassport);
        SetBeneficiaryPhone(beneficiaryPhone);
        SetBeneficiaryEmail(beneficiaryEmail);
        SetBeneficiaryProvinceId(beneficiaryProvinceId);
        SetBeneficiaryWardId(beneficiaryWardId);
        SetBeneficiaryAddress(beneficiaryAddress);
        SetBeneficiaryFullAddress(beneficiaryFullAddress);
        SetBeneficiaryOrgType(beneficiaryOrgType);
    }

    // Private setters with validation
    private void SetContractId(Guid? contractId)
    {
        ContractId = contractId;
    }

    private void SetLobId(Guid lobId)
    {
        if (lobId == Guid.Empty)
        {
            throw new ArgumentException("LobId cannot be empty.", nameof(lobId));
        }
        LobId = lobId;
    }

    private void SetPolicyNo(string policyNo)
    {
        if (string.IsNullOrWhiteSpace(policyNo))
        {
            throw new ArgumentException("PolicyNo cannot be null or empty.", nameof(policyNo));
        }
        if (policyNo.Length > 50)
        {
            throw new ArgumentException("PolicyNo cannot exceed 50 characters.", nameof(policyNo));
        }
        PolicyNo = policyNo;
    }

    private void SetLastVersionId(Guid lastVersionId)
    {
        if (lastVersionId == Guid.Empty)
        {
            throw new ArgumentException("LastVersionId cannot be empty.", nameof(lastVersionId));
        }
        LastVersionId = lastVersionId;
    }

    private void SetSellType(PolicySellType sellType)
    {
        SellType = sellType;
    }

    private void SetInsurerPolicyNo(string? insurerPolicyNo)
    {
        if (insurerPolicyNo != null && insurerPolicyNo.Length > 50)
        {
            throw new ArgumentException("InsurerPolicyNo cannot exceed 50 characters.", nameof(insurerPolicyNo));
        }
        InsurerPolicyNo = insurerPolicyNo;
    }

    private void SetPolicyTypeId(Guid policyTypeId)
    {
        if (policyTypeId == Guid.Empty)
        {
            throw new ArgumentException("PolicyTypeId cannot be empty.", nameof(policyTypeId));
        }
        PolicyTypeId = policyTypeId;
    }

    private void SetPartnerId(Guid partnerId)
    {
        if (partnerId == Guid.Empty)
        {
            throw new ArgumentException("PartnerId cannot be empty.", nameof(partnerId));
        }
        PartnerId = partnerId;
    }

    private void SetSellerId(Guid sellerId)
    {
        if (sellerId == Guid.Empty)
        {
            throw new ArgumentException("SellerId cannot be empty.", nameof(sellerId));
        }
        SellerId = sellerId;
    }

    private void SetImplementerId(Guid implementerId)
    {
        if (implementerId == Guid.Empty)
        {
            throw new ArgumentException("ImplementerId cannot be empty.", nameof(implementerId));
        }
        ImplementerId = implementerId;
    }

    private void SetCurrencyId(Guid currencyId)
    {
        if (currencyId == Guid.Empty)
        {
            throw new ArgumentException("CurrencyId cannot be empty.", nameof(currencyId));
        }
        CurrencyId = currencyId;
    }

    private void SetExchangeRate(decimal exchangeRate)
    {
        if (exchangeRate < 0)
        {
            throw new ArgumentException("ExchangeRate cannot be negative.", nameof(exchangeRate));
        }
        ExchangeRate = exchangeRate;
    }

    private void SetStatus(PolicyStatus status)
    {
        Status = status;
    }

    private void SetIssueDate(DateTime? issueDate)
    {
        IssueDate = issueDate;
    }

    private void SetApprovalStatus(string? approvalStatus)
    {
        if (approvalStatus != null && approvalStatus.Length > 15)
        {
            throw new ArgumentException("ApprovalStatus cannot exceed 15 characters.", nameof(approvalStatus));
        }
        ApprovalStatus = approvalStatus;
    }

    private void SetCancellationDate(DateTime? cancellationDate)
    {
        CancellationDate = cancellationDate;
    }

    private void SetTerminationDate(DateTime? terminationDate)
    {
        TerminationDate = terminationDate;
    }

    private void SetCancellationReasonId(Guid? cancellationReasonId)
    {
        CancellationReasonId = cancellationReasonId;
    }

    private void SetTerminationReasonId(Guid? terminationReasonId)
    {
        TerminationReasonId = terminationReasonId;
    }

    private void SetTerminationReasonDescription(string? terminationReasonDescription)
    {
        TerminationReasonDescription = terminationReasonDescription;
    }

    private void SetOrgEffectDate(DateTime orgEffectDate)
    {
        OrgEffectDate = orgEffectDate;
    }

    private void SetOrgExpireDate(DateTime orgExpireDate)
    {
        if (orgExpireDate < OrgEffectDate)
        {
            throw new ArgumentException("OrgExpireDate must be after OrgEffectDate.", nameof(orgExpireDate));
        }
        OrgExpireDate = orgExpireDate;
    }

    private void SetIsRenewal(string isRenewal)
    {
        if (string.IsNullOrWhiteSpace(isRenewal))
        {
            throw new ArgumentException("IsRenewal cannot be null or empty.", nameof(isRenewal));
        }
        if (isRenewal.Length > 1)
        {
            throw new ArgumentException("IsRenewal must be a single character (Y or N).", nameof(isRenewal));
        }
        if (isRenewal != "Y" && isRenewal != "N")
        {
            throw new ArgumentException("IsRenewal must be 'Y' or 'N'.", nameof(isRenewal));
        }
        IsRenewal = isRenewal;
    }

    private void SetIsGift(string isGift)
    {
        if (string.IsNullOrWhiteSpace(isGift))
        {
            throw new ArgumentException("IsGift cannot be null or empty.", nameof(isGift));
        }
        if (isGift.Length > 1)
        {
            throw new ArgumentException("IsGift must be a single character (Y or N).", nameof(isGift));
        }
        if (isGift != "Y" && isGift != "N")
        {
            throw new ArgumentException("IsGift must be 'Y' or 'N'.", nameof(isGift));
        }
        IsGift = isGift;
    }

    private void SetPremiumTotal(decimal premiumTotal)
    {
        if (premiumTotal < 0)
        {
            throw new ArgumentException("PremiumTotal cannot be negative.", nameof(premiumTotal));
        }
        PremiumTotal = premiumTotal;
    }

    private void SetPremium(decimal premium)
    {
        if (premium < 0)
        {
            throw new ArgumentException("Premium cannot be negative.", nameof(premium));
        }
        Premium = premium;
    }

    private void SetVat(decimal vat)
    {
        if (vat < 0)
        {
            throw new ArgumentException("Vat cannot be negative.", nameof(vat));
        }
        Vat = vat;
    }

    private void SetDiscount(decimal? discount)
    {
        if (discount.HasValue && discount.Value < 0)
        {
            throw new ArgumentException("Discount cannot be negative.", nameof(discount));
        }
        Discount = discount;
    }

    private void SetDiscountRate(decimal? discountRate)
    {
        if (discountRate.HasValue && discountRate.Value < 0)
        {
            throw new ArgumentException("DiscountRate cannot be negative.", nameof(discountRate));
        }
        DiscountRate = discountRate;
    }

    private void SetIsBankLoan(string isBankLoan)
    {
        if (string.IsNullOrWhiteSpace(isBankLoan))
        {
            throw new ArgumentException("IsBankLoan cannot be null or empty.", nameof(isBankLoan));
        }
        if (isBankLoan.Length > 1)
        {
            throw new ArgumentException("IsBankLoan must be a single character (Y or N).", nameof(isBankLoan));
        }
        if (isBankLoan != "Y" && isBankLoan != "N")
        {
            throw new ArgumentException("IsBankLoan must be 'Y' or 'N'.", nameof(isBankLoan));
        }
        IsBankLoan = isBankLoan;
    }

    private void SetChannelId(Guid? channelId)
    {
        ChannelId = channelId;
    }

    private void SetLotImportCode(string? lotImportCode)
    {
        if (lotImportCode != null && lotImportCode.Length > 50)
        {
            throw new ArgumentException("LotImportCode cannot exceed 50 characters.", nameof(lotImportCode));
        }
        LotImportCode = lotImportCode;
    }

    private void SetInsuredName(string? insuredName)
    {
        if (insuredName != null && insuredName.Length > 250)
        {
            throw new ArgumentException("InsuredName cannot exceed 250 characters.", nameof(insuredName));
        }
        InsuredName = insuredName;
    }

    private void SetInsuredIdNo(string? insuredIdNo)
    {
        if (insuredIdNo != null && insuredIdNo.Length > 15)
        {
            throw new ArgumentException("InsuredIdNo cannot exceed 15 characters.", nameof(insuredIdNo));
        }
        InsuredIdNo = insuredIdNo;
    }

    private void SetInsuredTin(string? insuredTin)
    {
        if (insuredTin != null && insuredTin.Length > 15)
        {
            throw new ArgumentException("InsuredTin cannot exceed 15 characters.", nameof(insuredTin));
        }
        InsuredTin = insuredTin;
    }

    private void SetInsuredPassport(string? insuredPassport)
    {
        if (insuredPassport != null && insuredPassport.Length > 25)
        {
            throw new ArgumentException("InsuredPassport cannot exceed 25 characters.", nameof(insuredPassport));
        }
        InsuredPassport = insuredPassport;
    }

    private void SetInsuredPhone(string? insuredPhone)
    {
        if (insuredPhone != null && insuredPhone.Length > 15)
        {
            throw new ArgumentException("InsuredPhone cannot exceed 15 characters.", nameof(insuredPhone));
        }
        InsuredPhone = insuredPhone;
    }

    private void SetInsuredEmail(string? insuredEmail)
    {
        if (insuredEmail != null && insuredEmail.Length > 50)
        {
            throw new ArgumentException("InsuredEmail cannot exceed 50 characters.", nameof(insuredEmail));
        }
        InsuredEmail = insuredEmail;
    }

    private void SetInsuredProvinceId(Guid? insuredProvinceId)
    {
        InsuredProvinceId = insuredProvinceId;
    }

    private void SetInsuredWardId(Guid? insuredWardId)
    {
        InsuredWardId = insuredWardId;
    }

    private void SetInsuredAddress(string? insuredAddress)
    {
        if (insuredAddress != null && insuredAddress.Length > 250)
        {
            throw new ArgumentException("InsuredAddress cannot exceed 250 characters.", nameof(insuredAddress));
        }
        InsuredAddress = insuredAddress;
    }

    private void SetInsuredFullAddress(string? insuredFullAddress)
    {
        if (insuredFullAddress != null && insuredFullAddress.Length > 500)
        {
            throw new ArgumentException("InsuredFullAddress cannot exceed 500 characters.", nameof(insuredFullAddress));
        }
        InsuredFullAddress = insuredFullAddress;
    }

    private void SetInsuredOrgType(string? insuredOrgType)
    {
        if (insuredOrgType != null && insuredOrgType.Length > 15)
        {
            throw new ArgumentException("InsuredOrgType cannot exceed 15 characters.", nameof(insuredOrgType));
        }
        InsuredOrgType = insuredOrgType;
    }

    private void SetBeneficiaryName(string? beneficiaryName)
    {
        if (beneficiaryName != null && beneficiaryName.Length > 250)
        {
            throw new ArgumentException("BeneficiaryName cannot exceed 250 characters.", nameof(beneficiaryName));
        }
        BeneficiaryName = beneficiaryName;
    }

    private void SetBeneficiaryIdNo(string? beneficiaryIdNo)
    {
        if (beneficiaryIdNo != null && beneficiaryIdNo.Length > 15)
        {
            throw new ArgumentException("BeneficiaryIdNo cannot exceed 15 characters.", nameof(beneficiaryIdNo));
        }
        BeneficiaryIdNo = beneficiaryIdNo;
    }

    private void SetBeneficiaryTin(string? beneficiaryTin)
    {
        if (beneficiaryTin != null && beneficiaryTin.Length > 15)
        {
            throw new ArgumentException("BeneficiaryTin cannot exceed 15 characters.", nameof(beneficiaryTin));
        }
        BeneficiaryTin = beneficiaryTin;
    }

    private void SetBeneficiaryPassport(string? beneficiaryPassport)
    {
        if (beneficiaryPassport != null && beneficiaryPassport.Length > 25)
        {
            throw new ArgumentException("BeneficiaryPassport cannot exceed 25 characters.", nameof(beneficiaryPassport));
        }
        BeneficiaryPassport = beneficiaryPassport;
    }

    private void SetBeneficiaryPhone(string? beneficiaryPhone)
    {
        if (beneficiaryPhone != null && beneficiaryPhone.Length > 15)
        {
            throw new ArgumentException("BeneficiaryPhone cannot exceed 15 characters.", nameof(beneficiaryPhone));
        }
        BeneficiaryPhone = beneficiaryPhone;
    }

    private void SetBeneficiaryEmail(string? beneficiaryEmail)
    {
        if (beneficiaryEmail != null && beneficiaryEmail.Length > 50)
        {
            throw new ArgumentException("BeneficiaryEmail cannot exceed 50 characters.", nameof(beneficiaryEmail));
        }
        BeneficiaryEmail = beneficiaryEmail;
    }

    private void SetBeneficiaryProvinceId(Guid? beneficiaryProvinceId)
    {
        BeneficiaryProvinceId = beneficiaryProvinceId;
    }

    private void SetBeneficiaryWardId(Guid? beneficiaryWardId)
    {
        BeneficiaryWardId = beneficiaryWardId;
    }

    private void SetBeneficiaryAddress(string? beneficiaryAddress)
    {
        if (beneficiaryAddress != null && beneficiaryAddress.Length > 250)
        {
            throw new ArgumentException("BeneficiaryAddress cannot exceed 250 characters.", nameof(beneficiaryAddress));
        }
        BeneficiaryAddress = beneficiaryAddress;
    }

    private void SetBeneficiaryFullAddress(string? beneficiaryFullAddress)
    {
        if (beneficiaryFullAddress != null && beneficiaryFullAddress.Length > 500)
        {
            throw new ArgumentException("BeneficiaryFullAddress cannot exceed 500 characters.", nameof(beneficiaryFullAddress));
        }
        BeneficiaryFullAddress = beneficiaryFullAddress;
    }

    private void SetBeneficiaryOrgType(string? beneficiaryOrgType)
    {
        if (beneficiaryOrgType != null && beneficiaryOrgType.Length > 15)
        {
            throw new ArgumentException("BeneficiaryOrgType cannot exceed 15 characters.", nameof(beneficiaryOrgType));
        }
        BeneficiaryOrgType = beneficiaryOrgType;
    }

    // Public update methods
    // ⚠️ QUAN TRỌNG: Không có method UpdatePolicyNo() - PolicyNo không được phép sửa (immutable)
    public virtual void UpdateContractId(Guid? contractId)
    {
        // contract is immutable by business rule for UpdateAsync; keep method for internal use if needed
        SetContractId(contractId);
    }

    public virtual void UpdateLastVersionId(Guid lastVersionId)
    {
        SetLastVersionId(lastVersionId);
    }

    public virtual void UpdateLobId(Guid lobId)
    {
        SetLobId(lobId);
    }

    public virtual void UpdateSellType(PolicySellType sellType)
    {
        SetSellType(sellType);
    }

    public virtual void UpdateInsurerPolicyNo(string? insurerPolicyNo)
    {
        SetInsurerPolicyNo(insurerPolicyNo);
    }

    public virtual void UpdatePolicyTypeId(Guid policyTypeId)
    {
        SetPolicyTypeId(policyTypeId);
    }

    public virtual void UpdatePartnerId(Guid partnerId)
    {
        SetPartnerId(partnerId);
    }

    public virtual void UpdateSellerId(Guid sellerId)
    {
        SetSellerId(sellerId);
    }

    public virtual void UpdateImplementerId(Guid implementerId)
    {
        SetImplementerId(implementerId);
    }

    public virtual void UpdateCurrencyId(Guid currencyId)
    {
        SetCurrencyId(currencyId);
    }

    public virtual void UpdateExchangeRate(decimal exchangeRate)
    {
        SetExchangeRate(exchangeRate);
    }

    public virtual void UpdateStatus(PolicyStatus status)
    {
        SetStatus(status);
    }

    public virtual void UpdateIssueDate(DateTime? issueDate)
    {
        SetIssueDate(issueDate);
    }

    public virtual void UpdateApprovalStatus(string? approvalStatus)
    {
        SetApprovalStatus(approvalStatus);
    }

    public virtual void UpdateCancellationDate(DateTime? cancellationDate)
    {
        SetCancellationDate(cancellationDate);
    }

    public virtual void UpdateTerminationDate(DateTime? terminationDate)
    {
        SetTerminationDate(terminationDate);
    }

    public virtual void UpdateCancellationReasonId(Guid? cancellationReasonId)
    {
        SetCancellationReasonId(cancellationReasonId);
    }

    public virtual void UpdateTerminationReasonId(Guid? terminationReasonId)
    {
        SetTerminationReasonId(terminationReasonId);
    }

    public virtual void UpdateTerminationReasonDescription(string? terminationReasonDescription)
    {
        SetTerminationReasonDescription(terminationReasonDescription);
    }

    public virtual void UpdateOrgEffectDate(DateTime orgEffectDate)
    {
        SetOrgEffectDate(orgEffectDate);
    }

    public virtual void UpdateOrgExpireDate(DateTime orgExpireDate)
    {
        SetOrgExpireDate(orgExpireDate);
    }

    public virtual void UpdateIsRenewal(string isRenewal)
    {
        SetIsRenewal(isRenewal);
    }

    public virtual void UpdateIsGift(string isGift)
    {
        SetIsGift(isGift);
    }

    public virtual void UpdatePremiumTotal(decimal premiumTotal)
    {
        SetPremiumTotal(premiumTotal);
    }

    public virtual void UpdatePremium(decimal premium)
    {
        SetPremium(premium);
    }

    public virtual void UpdateVat(decimal vat)
    {
        SetVat(vat);
    }

    public virtual void UpdateDiscount(decimal? discount)
    {
        SetDiscount(discount);
    }

    public virtual void UpdateDiscountRate(decimal? discountRate)
    {
        SetDiscountRate(discountRate);
    }

    public virtual void UpdateIsBankLoan(string isBankLoan)
    {
        SetIsBankLoan(isBankLoan);
    }

    public virtual void UpdateChannelId(Guid? channelId)
    {
        SetChannelId(channelId);
    }

    public virtual void UpdateLotImportCode(string? lotImportCode)
    {
        SetLotImportCode(lotImportCode);
    }

    public virtual void UpdateInsuredName(string? insuredName)
    {
        SetInsuredName(insuredName);
    }

    public virtual void UpdateInsuredIdNo(string? insuredIdNo)
    {
        SetInsuredIdNo(insuredIdNo);
    }

    public virtual void UpdateInsuredTin(string? insuredTin)
    {
        SetInsuredTin(insuredTin);
    }

    public virtual void UpdateInsuredPassport(string? insuredPassport)
    {
        SetInsuredPassport(insuredPassport);
    }

    public virtual void UpdateInsuredPhone(string? insuredPhone)
    {
        SetInsuredPhone(insuredPhone);
    }

    public virtual void UpdateInsuredEmail(string? insuredEmail)
    {
        SetInsuredEmail(insuredEmail);
    }

    public virtual void UpdateInsuredProvinceId(Guid? insuredProvinceId)
    {
        SetInsuredProvinceId(insuredProvinceId);
    }

    public virtual void UpdateInsuredWardId(Guid? insuredWardId)
    {
        SetInsuredWardId(insuredWardId);
    }

    public virtual void UpdateInsuredAddress(string? insuredAddress)
    {
        SetInsuredAddress(insuredAddress);
    }

    public virtual void UpdateInsuredFullAddress(string? insuredFullAddress)
    {
        SetInsuredFullAddress(insuredFullAddress);
    }

    public virtual void UpdateInsuredOrgType(string? insuredOrgType)
    {
        SetInsuredOrgType(insuredOrgType);
    }

    public virtual void UpdateBeneficiaryName(string? beneficiaryName)
    {
        SetBeneficiaryName(beneficiaryName);
    }

    public virtual void UpdateBeneficiaryIdNo(string? beneficiaryIdNo)
    {
        SetBeneficiaryIdNo(beneficiaryIdNo);
    }

    public virtual void UpdateBeneficiaryTin(string? beneficiaryTin)
    {
        SetBeneficiaryTin(beneficiaryTin);
    }

    public virtual void UpdateBeneficiaryPassport(string? beneficiaryPassport)
    {
        SetBeneficiaryPassport(beneficiaryPassport);
    }

    public virtual void UpdateBeneficiaryPhone(string? beneficiaryPhone)
    {
        SetBeneficiaryPhone(beneficiaryPhone);
    }

    public virtual void UpdateBeneficiaryEmail(string? beneficiaryEmail)
    {
        SetBeneficiaryEmail(beneficiaryEmail);
    }

    public virtual void UpdateBeneficiaryProvinceId(Guid? beneficiaryProvinceId)
    {
        SetBeneficiaryProvinceId(beneficiaryProvinceId);
    }

    public virtual void UpdateBeneficiaryWardId(Guid? beneficiaryWardId)
    {
        SetBeneficiaryWardId(beneficiaryWardId);
    }

    public virtual void UpdateBeneficiaryAddress(string? beneficiaryAddress)
    {
        SetBeneficiaryAddress(beneficiaryAddress);
    }

    public virtual void UpdateBeneficiaryFullAddress(string? beneficiaryFullAddress)
    {
        SetBeneficiaryFullAddress(beneficiaryFullAddress);
    }

    public virtual void UpdateBeneficiaryOrgType(string? beneficiaryOrgType)
    {
        SetBeneficiaryOrgType(beneficiaryOrgType);
    }
}
