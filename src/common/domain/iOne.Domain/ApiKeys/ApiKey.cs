using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace iOne.ApiKeys;

[Table("api_keys")]
public class ApiKey : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public virtual Guid? TenantId { get; protected set; }
    public virtual Guid UserId { get; protected set; }
    public virtual string Name { get; protected set; } = null!;
    public virtual string Prefix { get; protected set; } = null!;
    public virtual string KeyHash { get; protected set; } = null!;
    public virtual DateTime? ExpiresAt { get; protected set; }
    public virtual bool IsActive { get; protected set; }

    protected ApiKey()
    {
        // For ORM
    }

    public ApiKey(
        Guid id,
        Guid userId,
        string name,
        string prefix,
        string keyHash,
        DateTime? expiresAt,
        Guid? tenantId)
        : base(id)
    {
        UserId = userId;
        SetName(name);
        Prefix = prefix ?? throw new ArgumentNullException(nameof(prefix));
        KeyHash = keyHash ?? throw new ArgumentNullException(nameof(keyHash));
        ExpiresAt = expiresAt;
        IsActive = true;
        TenantId = tenantId;
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or empty.", nameof(name));
        if (name.Length > 256)
            throw new ArgumentException("Name cannot exceed 256 characters.", nameof(name));
        Name = name;
    }

    public virtual void SetActive(bool isActive)
    {
        IsActive = isActive;
    }

    public virtual void SetExpiresAt(DateTime? expiresAt)
    {
        ExpiresAt = expiresAt;
    }
}
