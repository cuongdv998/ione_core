using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using Volo.Abp.Domain.Entities.Auditing;
using iOne.ProRuleTypes;

namespace iOne.ProRules;

[Table("pro_rule")]
public class ProRule : FullAuditedAggregateRoot<Guid>
{
    [Required]
    [MaxLength(15)]
    public virtual string ApplyTo { get; private set; } = null!;

    [Required]
    public virtual Guid ApplyToId { get; private set; }

    [Required]
    public virtual Guid RuleTypeId { get; private set; }

    [Required]
    [MaxLength(50)]
    public virtual string Code { get; private set; } = null!;

    [Required]
    [MaxLength(250)]
    public virtual string Name { get; private set; } = null!;

    [MaxLength(500)]
    public virtual string? Description { get; private set; }

    [Required]
    public virtual string RuleScript { get; private set; } = null!;

    [Required]
    public virtual int Priority { get; private set; } = 1;

    [Required]
    public virtual ProRuleStatus Status { get; private set; }

    [Required]
    public virtual DateTime EffectDate { get; private set; }

    public virtual DateTime? ExpireDate { get; private set; }

    // Navigation property for many-to-one relationship
    public virtual ProRuleType? RuleType { get; set; }

    protected ProRule()
    {
        // For ORM
    }

    public ProRule(
        Guid id,
        string applyTo,
        Guid applyToId,
        Guid ruleTypeId,
        string code,
        string name,
        string ruleScript,
        ProRuleStatus status,
        DateTime effectDate,
        string? description = null,
        int priority = 1,
        DateTime? expireDate = null)
        : base(id)
    {
        SetApplyTo(applyTo);
        SetApplyToId(applyToId);
        SetRuleTypeId(ruleTypeId);
        SetCode(code);
        SetName(name);
        SetRuleScript(ruleScript);
        SetStatus(status);
        SetEffectDate(effectDate);
        SetDescription(description);
        SetPriority(priority);
        SetExpireDate(expireDate);
    }

    private void SetApplyTo(string applyTo)
    {
        if (string.IsNullOrWhiteSpace(applyTo))
        {
            throw new ArgumentException("ApplyTo cannot be null or empty.", nameof(applyTo));
        }

        if (applyTo.Length > 15)
        {
            throw new ArgumentException("ApplyTo cannot exceed 15 characters.", nameof(applyTo));
        }

        ApplyTo = applyTo;
    }

    private void SetApplyToId(Guid applyToId)
    {
        if (applyToId == Guid.Empty)
        {
            throw new ArgumentException("ApplyToId cannot be empty.", nameof(applyToId));
        }

        ApplyToId = applyToId;
    }

    private void SetRuleTypeId(Guid ruleTypeId)
    {
        if (ruleTypeId == Guid.Empty)
        {
            throw new ArgumentException("RuleTypeId cannot be empty.", nameof(ruleTypeId));
        }

        RuleTypeId = ruleTypeId;
    }

    private void SetCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Code cannot be null or empty.", nameof(code));
        }

        if (code.Length > 50)
        {
            throw new ArgumentException("Code cannot exceed 50 characters.", nameof(code));
        }

        // Convert to uppercase and validate format: only A-Z, 0-9, and underscore
        var upperCode = code.ToUpperInvariant();
        if (!Regex.IsMatch(upperCode, @"^[A-Z0-9_]+$"))
        {
            throw new ArgumentException("Code can only contain letters (A-Z), numbers (0-9), and underscore (_).", nameof(code));
        }

        Code = upperCode;
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name cannot be null or empty.", nameof(name));
        }

        if (name.Length > 250)
        {
            throw new ArgumentException("Name cannot exceed 250 characters.", nameof(name));
        }

        Name = name;
    }

    private void SetDescription(string? description)
    {
        if (description != null && description.Length > 500)
        {
            throw new ArgumentException("Description cannot exceed 500 characters.", nameof(description));
        }

        Description = description;
    }

    private void SetRuleScript(string ruleScript)
    {
        if (string.IsNullOrWhiteSpace(ruleScript))
        {
            throw new ArgumentException("RuleScript cannot be null or empty.", nameof(ruleScript));
        }

        RuleScript = ruleScript;
    }

    private void SetPriority(int priority)
    {
        if (priority < 1 || priority > 999)
        {
            throw new ArgumentException("Priority must be between 1 and 999.", nameof(priority));
        }

        Priority = priority;
    }

    private void SetStatus(ProRuleStatus status)
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

    // ⚠️ QUAN TRỌNG: Không có method UpdateCode() - Code không được phép sửa
    public virtual void UpdateApplyTo(string applyTo)
    {
        SetApplyTo(applyTo);
    }

    public virtual void UpdateApplyToId(Guid applyToId)
    {
        SetApplyToId(applyToId);
    }

    public virtual void UpdateRuleTypeId(Guid ruleTypeId)
    {
        SetRuleTypeId(ruleTypeId);
    }

    public virtual void UpdateName(string name)
    {
        SetName(name);
    }

    public virtual void UpdateDescription(string? description)
    {
        SetDescription(description);
    }

    public virtual void UpdateRuleScript(string ruleScript)
    {
        SetRuleScript(ruleScript);
    }

    public virtual void UpdatePriority(int priority)
    {
        SetPriority(priority);
    }

    public virtual void UpdateStatus(ProRuleStatus status)
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
