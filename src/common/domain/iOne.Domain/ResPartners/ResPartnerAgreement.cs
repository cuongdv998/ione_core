using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;

namespace iOne.ResPartners;

[Table("res_partner_agreement")]
public class ResPartnerAgreement : AuditedEntity<Guid>
{
    public virtual Guid PartnerId { get; private set; }
    public virtual Guid AgreementTermId { get; private set; }
    public virtual string Value { get; private set; } = null!;
    public virtual DateTime? EffectDate { get; private set; }
    public virtual DateTime ExpireDate { get; private set; }

    // Navigation Properties
    public virtual ResPartner Partner { get; set; } = null!;
    public virtual ResAgreementTerms.ResAgreementTerm AgreementTerm { get; set; } = null!;

    protected ResPartnerAgreement()
    {
        // For ORM
    }

    public ResPartnerAgreement(
        Guid id,
        Guid partnerId,
        Guid agreementTermId,
        string value,
        DateTime expireDate,
        DateTime? effectDate = null)
        : base(id)
    {
        SetPartnerId(partnerId);
        SetAgreementTermId(agreementTermId);
        SetValue(value);
        SetExpireDate(expireDate);
        SetEffectDate(effectDate);
    }

    private void SetPartnerId(Guid partnerId)
    {
        if (partnerId == Guid.Empty)
        {
            throw new ArgumentException("PartnerId cannot be empty.", nameof(partnerId));
        }
        PartnerId = partnerId;
    }

    private void SetAgreementTermId(Guid agreementTermId)
    {
        if (agreementTermId == Guid.Empty)
        {
            throw new ArgumentException("AgreementTermId cannot be empty.", nameof(agreementTermId));
        }
        AgreementTermId = agreementTermId;
    }

    private void SetValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value cannot be null or empty.", nameof(value));
        }

        if (value.Length > 50)
        {
            throw new ArgumentException("Value cannot exceed 50 characters.", nameof(value));
        }

        Value = value;
    }

    private void SetEffectDate(DateTime? effectDate)
    {
        EffectDate = effectDate;
    }

    private void SetExpireDate(DateTime expireDate)
    {
        if (expireDate == default)
        {
            throw new ArgumentException("ExpireDate cannot be default value.", nameof(expireDate));
        }

        // Validate: ExpireDate must be after EffectDate (if EffectDate is provided)
        if (EffectDate.HasValue && expireDate < EffectDate.Value)
        {
            throw new ArgumentException("ExpireDate must be after or equal to EffectDate.", nameof(expireDate));
        }

        ExpireDate = expireDate;
    }

    public virtual void UpdateValue(string value)
    {
        SetValue(value);
    }

    public virtual void UpdateEffectDate(DateTime? effectDate)
    {
        SetEffectDate(effectDate);
        // Re-validate ExpireDate after EffectDate change
        if (effectDate.HasValue && ExpireDate < effectDate.Value)
        {
            throw new ArgumentException("ExpireDate must be after or equal to EffectDate.", nameof(effectDate));
        }
    }

    public virtual void UpdateExpireDate(DateTime expireDate)
    {
        SetExpireDate(expireDate);
    }
}

