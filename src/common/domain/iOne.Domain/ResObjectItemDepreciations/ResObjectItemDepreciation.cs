using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;
using iOne.ResObjectTypeItems;
using iOne.ResCarGroups;

namespace iOne.ResObjectItemDepreciations;

[Table("res_object_item_depreciation")]
public class ResObjectItemDepreciation : FullAuditedAggregateRoot<Guid>
{
    [Required]
    public virtual Guid ObjectTypeItemId { get; private set; }

    [Required]
    public virtual Guid CarGroupId { get; private set; }

    [Required]
    public virtual double UsedTimeFrom { get; private set; }

    [Required]
    public virtual double UsedTimeTo { get; private set; }

    [Required]
    public virtual double DepreciationPercent { get; private set; }

    [Required]
    public virtual DateTime EffectDate { get; private set; }

    public virtual DateTime? ExpireDate { get; private set; }

    [Required]
    public virtual ResObjectItemDepreciationStatus Status { get; private set; }

    // Navigation properties for many-to-one relationships
    public virtual ResObjectTypeItem? ObjectTypeItemNavigation { get; private set; }
    
    public virtual ResCarGroup? CarGroupNavigation { get; private set; }

    protected ResObjectItemDepreciation()
    {
        // For ORM
    }

    public ResObjectItemDepreciation(
        Guid id,
        Guid objectTypeItemId,
        Guid carGroupId,
        double usedTimeFrom,
        double usedTimeTo,
        double depreciationPercent,
        DateTime effectDate,
        DateTime? expireDate,
        ResObjectItemDepreciationStatus status)
        : base(id)
    {
        SetObjectTypeItemId(objectTypeItemId);
        SetCarGroupId(carGroupId);
        SetUsedTimeRange(usedTimeFrom, usedTimeTo);
        SetDepreciationPercent(depreciationPercent);
        SetEffectDate(effectDate);
        SetExpireDate(expireDate);
        SetStatus(status);
    }

    private void SetObjectTypeItemId(Guid objectTypeItemId)
    {
        if (objectTypeItemId == Guid.Empty)
        {
            throw new ArgumentException("ObjectTypeItemId cannot be empty.", nameof(objectTypeItemId));
        }

        ObjectTypeItemId = objectTypeItemId;
    }

    private void SetCarGroupId(Guid carGroupId)
    {
        if (carGroupId == Guid.Empty)
        {
            throw new ArgumentException("CarGroupId cannot be empty.", nameof(carGroupId));
        }

        CarGroupId = carGroupId;
    }

    private void SetUsedTimeRange(double usedTimeFrom, double usedTimeTo)
    {
        if (usedTimeFrom < 0)
        {
            throw new ArgumentException("UsedTimeFrom cannot be negative.", nameof(usedTimeFrom));
        }

        if (usedTimeTo < 0)
        {
            throw new ArgumentException("UsedTimeTo cannot be negative.", nameof(usedTimeTo));
        }

        if (usedTimeFrom > usedTimeTo)
        {
            throw new ArgumentException("UsedTimeFrom must be less than or equal to UsedTimeTo.", nameof(usedTimeFrom));
        }

        UsedTimeFrom = usedTimeFrom;
        UsedTimeTo = usedTimeTo;
    }

    private void SetDepreciationPercent(double depreciationPercent)
    {
        if (depreciationPercent < 0 || depreciationPercent > 100)
        {
            throw new ArgumentException("DepreciationPercent must be between 0 and 100.", nameof(depreciationPercent));
        }

        DepreciationPercent = depreciationPercent;
    }

    private void SetEffectDate(DateTime effectDate)
    {
        EffectDate = effectDate;
    }

    private void SetExpireDate(DateTime? expireDate)
    {
        if (expireDate.HasValue && expireDate.Value <= EffectDate)
        {
            throw new ArgumentException("ExpireDate must be after EffectDate.", nameof(expireDate));
        }

        ExpireDate = expireDate;
    }

    private void SetStatus(ResObjectItemDepreciationStatus status)
    {
        Status = status;
    }

    public virtual void UpdateObjectTypeItemId(Guid objectTypeItemId)
    {
        SetObjectTypeItemId(objectTypeItemId);
    }

    public virtual void UpdateCarGroupId(Guid carGroupId)
    {
        SetCarGroupId(carGroupId);
    }

    public virtual void UpdateUsedTimeRange(double usedTimeFrom, double usedTimeTo)
    {
        SetUsedTimeRange(usedTimeFrom, usedTimeTo);
    }

    public virtual void UpdateDepreciationPercent(double depreciationPercent)
    {
        SetDepreciationPercent(depreciationPercent);
    }

    public virtual void UpdateEffectDate(DateTime effectDate)
    {
        SetEffectDate(effectDate);
    }

    public virtual void UpdateExpireDate(DateTime? expireDate)
    {
        SetExpireDate(expireDate);
    }

    public virtual void UpdateStatus(ResObjectItemDepreciationStatus status)
    {
        SetStatus(status);
    }
}
