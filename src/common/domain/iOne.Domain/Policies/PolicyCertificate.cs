using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;

namespace iOne.Policies;

[Table("policy_certificate")]
public class PolicyCertificate : FullAuditedAggregateRoot<Guid>, IEntity<Guid>
{
    [Required]
    public virtual Guid PolicyId { get; private set; }

    [Required]
    public virtual Guid PolicyVersionId { get; private set; }

    [MaxLength(50)]
    public virtual string? CertificateNo { get; private set; }

    [MaxLength(250)]
    public virtual string? Url { get; private set; }

    // Navigation Properties
    public virtual Policy Policy { get; set; } = null!;
    public virtual PolicyVersion PolicyVersion { get; set; } = null!;

    protected PolicyCertificate()
    {
        // For ORM
    }

    public PolicyCertificate(
        Guid id,
        Guid policyId,
        Guid policyVersionId,
        string? certificateNo = null,
        string? url = null)
        : base(id)
    {
        SetPolicyId(policyId);
        SetPolicyVersionId(policyVersionId);
        SetCertificateNo(certificateNo);
        SetUrl(url);
    }

    // Private setters with validation
    private void SetPolicyId(Guid policyId)
    {
        if (policyId == Guid.Empty)
        {
            throw new ArgumentException("PolicyId cannot be empty.", nameof(policyId));
        }
        PolicyId = policyId;
    }

    private void SetPolicyVersionId(Guid policyVersionId)
    {
        if (policyVersionId == Guid.Empty)
        {
            throw new ArgumentException("PolicyVersionId cannot be empty.", nameof(policyVersionId));
        }
        PolicyVersionId = policyVersionId;
    }

    private void SetCertificateNo(string? certificateNo)
    {
        if (certificateNo != null && certificateNo.Length > 50)
        {
            throw new ArgumentException("CertificateNo cannot exceed 50 characters.", nameof(certificateNo));
        }
        CertificateNo = certificateNo;
    }

    private void SetUrl(string? url)
    {
        if (url != null && url.Length > 250)
        {
            throw new ArgumentException("Url cannot exceed 250 characters.", nameof(url));
        }
        Url = url;
    }

    // Public update methods
    public virtual void UpdateCertificateNo(string? certificateNo)
    {
        SetCertificateNo(certificateNo);
    }

    public virtual void UpdateUrl(string? url)
    {
        SetUrl(url);
    }
}
