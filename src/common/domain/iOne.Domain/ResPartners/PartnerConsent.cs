using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace iOne.ResPartners;

/// <summary>
/// Bảng lưu lịch sử đồng ý điều khoản của đối tác
/// </summary>
public class PartnerConsent : AuditedEntity<Guid>
{
    public virtual Guid CustomerId { get; private set; }
    public virtual Guid? PolicyId { get; private set; }
    
    /// <summary>
    /// Loại điều khoản:
    /// DATA_SHARING: chia sẻ thông tin cá nhân
    /// CONTRACT_TERM: điều khoản hợp đồng
    /// </summary>
    public virtual string ConsentType { get; private set; } = null!;
    public virtual string? ConsentContent { get; private set; }
    public virtual string? TermVersion { get; private set; }
    public virtual bool IsAccepted { get; private set; }
    public virtual DateTime? AcceptedAt { get; private set; }
    public virtual string? CreatedBy { get; private set; }
    public virtual Guid? PartnerContractId { get; private set; }

    protected PartnerConsent() { }

    public PartnerConsent(
        Guid id,
        Guid customerId,
        string consentType,
        bool isAccepted,
        Guid? policyId = null,
        string? consentContent = null,
        string? termVersion = null,
        DateTime? acceptedAt = null,
        string? createdBy = null,
        Guid? partnerContractId = null) : base(id)
    {
        CustomerId = customerId;
        ConsentType = consentType;
        IsAccepted = isAccepted;
        PolicyId = policyId;
        ConsentContent = consentContent;
        TermVersion = termVersion;
        AcceptedAt = acceptedAt;
        CreatedBy = createdBy;
        PartnerContractId = partnerContractId;
    }
}
