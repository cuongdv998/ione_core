using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using iOne.ProTableRates;
using iOne.ProCoverages;
using iOne.ResChannels;
using iOne.ResPartners;
using Volo.Abp.Domain.Entities.Auditing;

namespace iOne.ProTableRateLines;

[Table("pro_table_rate_line")]
public class ProTableRateLine : FullAuditedAggregateRoot<Guid>
{
    [Required]
    public virtual Guid TableRateId { get; private set; }

    public virtual Guid? CoverageId { get; private set; }

    public virtual Guid? ChannelId { get; private set; }

    public virtual Guid? PartnerId { get; private set; }

    [MaxLength(250)]
    public virtual string? Name { get; private set; }

    [Required]
    public virtual string Condition { get; private set; } = null!;

    public virtual decimal? MinimumRate { get; private set; }

    public virtual decimal? BaseRate { get; private set; }

    public virtual decimal? FlatRate { get; private set; }

    public virtual decimal? MaxDiscount { get; private set; }

    public virtual decimal? LoadingRate { get; private set; }

    /// <summary>Tăng/giảm phí - numeric(15,3)</summary>
    public virtual decimal? Loading { get; private set; }

    [Required]
    public virtual DateTime EffectDate { get; private set; }

    public virtual DateTime? ExpireDate { get; private set; }

    // Navigation properties
    public virtual ProTableRate? TableRate { get; set; }
    public virtual ProCoverage? Coverage { get; set; }
    public virtual ResChannel? Channel { get; set; }
    public virtual ResPartner? Partner { get; set; }

    protected ProTableRateLine()
    {
        // For ORM
    }

    public ProTableRateLine(
        Guid id,
        Guid tableRateId,
        string? name,
        string condition,
        DateTime effectDate,
        Guid? coverageId = null,
        Guid? channelId = null,
        Guid? partnerId = null,
        decimal? minimumRate = null,
        decimal? baseRate = null,
        decimal? flatRate = null,
        decimal? maxDiscount = null,
        decimal? loadingRate = null,
        decimal? loading = null,
        DateTime? expireDate = null)
        : base(id)
    {
        SetTableRateId(tableRateId);
        SetName(name);
        SetCondition(condition);
        SetEffectDate(effectDate);
        SetCoverageId(coverageId);
        SetChannelId(channelId);
        SetPartnerId(partnerId);
        SetMinimumRate(minimumRate);
        SetBaseRate(baseRate);
        SetFlatRate(flatRate);
        SetMaxDiscount(maxDiscount);
        SetLoadingRate(loadingRate);
        SetLoading(loading);
        SetExpireDate(expireDate);
    }

    private void SetTableRateId(Guid tableRateId)
    {
        if (tableRateId == Guid.Empty)
        {
            throw new ArgumentException("TableRateId cannot be empty.", nameof(tableRateId));
        }

        TableRateId = tableRateId;
    }

    private void SetName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            Name = null;
            return;
        }

        var trimmed = name.Trim();
        if (trimmed.Length > 250)
        {
            throw new ArgumentException("Name cannot exceed 250 characters.", nameof(name));
        }

        Name = trimmed;
    }

    private void SetCondition(string condition)
    {
        if (string.IsNullOrWhiteSpace(condition))
        {
            throw new ArgumentException("Condition cannot be null or empty.", nameof(condition));
        }

        Condition = condition;
    }

    private void SetCoverageId(Guid? coverageId)
    {
        if (coverageId.HasValue && coverageId.Value == Guid.Empty)
        {
            throw new ArgumentException("CoverageId cannot be empty if provided.", nameof(coverageId));
        }

        CoverageId = coverageId;
    }

    private void SetChannelId(Guid? channelId)
    {
        if (channelId.HasValue && channelId.Value == Guid.Empty)
        {
            throw new ArgumentException("ChannelId cannot be empty if provided.", nameof(channelId));
        }

        ChannelId = channelId;
    }

    private void SetPartnerId(Guid? partnerId)
    {
        if (partnerId.HasValue && partnerId.Value == Guid.Empty)
        {
            throw new ArgumentException("PartnerId cannot be empty if provided.", nameof(partnerId));
        }

        PartnerId = partnerId;
    }

    private void SetMinimumRate(decimal? minimumRate)
    {
        if (minimumRate.HasValue && minimumRate.Value < 0)
        {
            throw new ArgumentException("MinimumRate cannot be negative.", nameof(minimumRate));
        }

        MinimumRate = minimumRate;
    }

    private void SetBaseRate(decimal? baseRate)
    {
        if (baseRate.HasValue && baseRate.Value < 0)
        {
            throw new ArgumentException("BaseRate cannot be negative.", nameof(baseRate));
        }

        BaseRate = baseRate;
    }

    private void SetFlatRate(decimal? flatRate)
    {
        if (flatRate.HasValue && flatRate.Value < 0)
        {
            throw new ArgumentException("FlatRate cannot be negative.", nameof(flatRate));
        }

        FlatRate = flatRate;
    }

    private void SetMaxDiscount(decimal? maxDiscount)
    {
        if (maxDiscount.HasValue && maxDiscount.Value < 0)
        {
            throw new ArgumentException("MaxDiscount cannot be negative.", nameof(maxDiscount));
        }

        MaxDiscount = maxDiscount;
    }

    private void SetLoadingRate(decimal? loadingRate)
    {
        if (loadingRate.HasValue && loadingRate.Value < 0)
        {
            throw new ArgumentException("LoadingRate cannot be negative.", nameof(loadingRate));
        }

        LoadingRate = loadingRate;
    }

    private void SetLoading(decimal? loading)
    {
        Loading = loading;
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

    public virtual void UpdateName(string? name)
    {
        SetName(name);
    }

    public virtual void UpdateCondition(string condition)
    {
        SetCondition(condition);
    }

    public virtual void UpdateTableRateId(Guid tableRateId)
    {
        SetTableRateId(tableRateId);
    }

    public virtual void UpdateCoverageId(Guid? coverageId)
    {
        SetCoverageId(coverageId);
    }

    public virtual void UpdateChannelId(Guid? channelId)
    {
        SetChannelId(channelId);
    }

    public virtual void UpdatePartnerId(Guid? partnerId)
    {
        SetPartnerId(partnerId);
    }

    public virtual void UpdateMinimumRate(decimal? minimumRate)
    {
        SetMinimumRate(minimumRate);
    }

    public virtual void UpdateBaseRate(decimal? baseRate)
    {
        SetBaseRate(baseRate);
    }

    public virtual void UpdateFlatRate(decimal? flatRate)
    {
        SetFlatRate(flatRate);
    }

    public virtual void UpdateMaxDiscount(decimal? maxDiscount)
    {
        SetMaxDiscount(maxDiscount);
    }

    public virtual void UpdateLoadingRate(decimal? loadingRate)
    {
        SetLoadingRate(loadingRate);
    }

    public virtual void UpdateLoading(decimal? loading)
    {
        SetLoading(loading);
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
