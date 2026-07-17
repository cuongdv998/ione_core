using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;

namespace iOne.InsurerDictionaries;

[Table("insurer_dictionary")]
public class InsurerDictionary : FullAuditedAggregateRoot<Guid>
{
    [Required]
    [MaxLength(50)]
    public virtual string BusinessName { get; private set; } = null!;

    [Required]
    public virtual Guid InsurerId { get; private set; }

    [Required]
    [MaxLength(50)]
    public virtual string OwnCode { get; private set; } = null!;

    [Required]
    [MaxLength(50)]
    public virtual string InsurerCode { get; private set; } = null!;

    public virtual string? ExtraData { get; private set; }

    [Required]
    public virtual InsurerDictionaryStatus Status { get; private set; }

    [Required]
    public virtual DateTime EffectDate { get; private set; }

    public virtual DateTime? ExpireDate { get; private set; }

    protected InsurerDictionary()
    {
        // For ORM
    }

    public InsurerDictionary(
        Guid id,
        string businessName,
        Guid insurerId,
        string ownCode,
        string insurerCode,
        string? extraData,
        InsurerDictionaryStatus status,
        DateTime effectDate,
        DateTime? expireDate)
        : base(id)
    {
        SetBusinessName(businessName);
        InsurerId = insurerId;
        SetOwnCode(ownCode);
        SetInsurerCode(insurerCode);
        SetExtraData(extraData);
        SetStatus(status);
        SetEffectDate(effectDate);
        SetExpireDate(expireDate);
    }

    private void SetBusinessName(string businessName)
    {
        if (string.IsNullOrWhiteSpace(businessName))
        {
            throw new ArgumentException("BusinessName cannot be null or empty.", nameof(businessName));
        }

        if (businessName.Length > 50)
        {
            throw new ArgumentException("BusinessName cannot exceed 50 characters.", nameof(businessName));
        }

        BusinessName = businessName;
    }

    private void SetOwnCode(string ownCode)
    {
        if (string.IsNullOrWhiteSpace(ownCode))
        {
            throw new ArgumentException("OwnCode cannot be null or empty.", nameof(ownCode));
        }

        if (ownCode.Length > 50)
        {
            throw new ArgumentException("OwnCode cannot exceed 50 characters.", nameof(ownCode));
        }

        OwnCode = ownCode;
    }

    private void SetInsurerCode(string insurerCode)
    {
        if (string.IsNullOrWhiteSpace(insurerCode))
        {
            throw new ArgumentException("InsurerCode cannot be null or empty.", nameof(insurerCode));
        }

        if (insurerCode.Length > 50)
        {
            throw new ArgumentException("InsurerCode cannot exceed 50 characters.", nameof(insurerCode));
        }

        InsurerCode = insurerCode;
    }

    private void SetExtraData(string? extraData)
    {
        ExtraData = extraData;
    }

    private void SetStatus(InsurerDictionaryStatus status)
    {
        Status = status;
    }

    private void SetEffectDate(DateTime effectDate)
    {
        EffectDate = effectDate;
    }

    private void SetExpireDate(DateTime? expireDate)
    {
        if (expireDate.HasValue && expireDate.Value < EffectDate)
        {
            throw new ArgumentException("ExpireDate cannot be earlier than EffectDate.", nameof(expireDate));
        }

        ExpireDate = expireDate;
    }

    // BusinessName, InsurerId, OwnCode are immutable after create (not updatable)

    public virtual void UpdateInsurerCode(string insurerCode)
    {
        SetInsurerCode(insurerCode);
    }

    public virtual void UpdateExtraData(string? extraData)
    {
        SetExtraData(extraData);
    }

    public virtual void UpdateStatus(InsurerDictionaryStatus status)
    {
        SetStatus(status);
    }

    public virtual void UpdateEffectDate(DateTime effectDate)
    {
        SetEffectDate(effectDate);
    }

    public virtual void UpdateExpireDate(DateTime? expireDate)
    {
        SetExpireDate(expireDate);
    }
}
