using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;

namespace iOne.ResClaimEvaluateItems;

[Table("res_claim_evaluate_item")]
public class ResClaimEvaluateItem : FullAuditedAggregateRoot<Guid>
{
    [Required]
    public virtual Guid ClaimTypeId { get; private set; }

    [Required]
    [MaxLength(25)]
    public virtual string Code { get; private set; } = null!;

    [Required]
    [MaxLength(250)]
    public virtual string Name { get; private set; } = null!;

    [Required]
    [MaxLength(10)]
    public virtual string Status { get; private set; } = null!;

    protected ResClaimEvaluateItem()
    {
    }

    public ResClaimEvaluateItem(
        Guid id,
        Guid claimTypeId,
        string code,
        string name,
        string status)
        : base(id)
    {
        UpdateClaimTypeId(claimTypeId);
        UpdateCode(code);
        UpdateName(name);
        UpdateStatus(status);
    }

    public virtual void UpdateClaimTypeId(Guid claimTypeId)
    {
        if (claimTypeId == Guid.Empty)
        {
            throw new ArgumentException("ClaimTypeId cannot be empty.", nameof(claimTypeId));
        }

        ClaimTypeId = claimTypeId;
    }

    public virtual void UpdateCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Code cannot be null or empty.", nameof(code));
        }

        var normalized = code.Trim();
        if (normalized.Length > 25)
        {
            throw new ArgumentException("Code cannot exceed 25 characters.", nameof(code));
        }

        Code = normalized;
    }

    public virtual void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name cannot be null or empty.", nameof(name));
        }

        if (name.Length > 250)
        {
            throw new ArgumentException("Name cannot exceed 250 characters.", nameof(name));
        }

        Name = name.Trim();
    }

    public virtual void UpdateStatus(string status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            throw new ArgumentException("Status cannot be null or empty.", nameof(status));
        }

        var normalized = status.Trim().ToLowerInvariant();
        if (normalized is not ("active" or "deactive"))
        {
            throw new ArgumentException("Status must be active or deactive.", nameof(status));
        }

        Status = normalized;
    }
}
