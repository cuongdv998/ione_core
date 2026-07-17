using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using Volo.Abp.Domain.Entities.Auditing;

namespace iOne.AdminConfigs;

[Table("admin_config")]
public class AdminConfig : FullAuditedAggregateRoot<Guid>
{
    [Required]
    [MaxLength(25)]
    public virtual string Code { get; private set; } = null!;

    [Required]
    [MaxLength(250)]
    public virtual string Name { get; private set; } = null!;

    [Required]
    [MaxLength(50)]
    public virtual string SubCode { get; private set; } = null!;

    [Required]
    [MaxLength(250)]
    public virtual string Value { get; private set; } = null!;

    [MaxLength(250)]
    public virtual string? Description { get; private set; }

    [Required]
    public virtual AdminConfigStatus Status { get; private set; }

    protected AdminConfig()
    {
        // For ORM
    }

    public AdminConfig(
        Guid id,
        string code,
        string name,
        string subCode,
        string value,
        AdminConfigStatus status,
        string? description = null)
        : base(id)
    {
        SetCode(code);
        SetName(name);
        SetSubCode(subCode);
        SetValue(value);
        SetStatus(status);
        SetDescription(description);
    }

    private void SetCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Code cannot be null or empty.", nameof(code));
        }

        if (code.Length > 25)
        {
            throw new ArgumentException("Code cannot exceed 25 characters.", nameof(code));
        }

        // Convert to uppercase and validate format: only A-Z, 0-9, and underscore
        var upperCode = code.ToUpperInvariant();
        if (!Regex.IsMatch(upperCode, @"^[A-Z0-9_]+$"))
        {
            throw new ArgumentException("Code can only contain letters (A-Z), numbers (0-9), and underscore (_).", nameof(code));
        }

        Code = upperCode;
    }

    private void SetSubCode(string subCode)
    {
        if (string.IsNullOrWhiteSpace(subCode))
        {
            throw new ArgumentException("SubCode cannot be null or empty.", nameof(subCode));
        }

        if (subCode.Length > 50)
        {
            throw new ArgumentException("SubCode cannot exceed 50 characters.", nameof(subCode));
        }

        // Convert to uppercase and validate format: only A-Z, 0-9, and underscore
        var upperSubCode = subCode.ToUpperInvariant();
        if (!Regex.IsMatch(upperSubCode, @"^[A-Z0-9_]+$"))
        {
            throw new ArgumentException("SubCode can only contain letters (A-Z), numbers (0-9), and underscore (_).", nameof(subCode));
        }

        SubCode = upperSubCode;
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

    private void SetValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value cannot be null or empty.", nameof(value));
        }

        if (value.Length > 250)
        {
            throw new ArgumentException("Value cannot exceed 250 characters.", nameof(value));
        }

        Value = value;
    }

    private void SetDescription(string? description)
    {
        if (!string.IsNullOrWhiteSpace(description) && description.Length > 250)
        {
            throw new ArgumentException("Description cannot exceed 250 characters.", nameof(description));
        }

        Description = description;
    }

    private void SetStatus(AdminConfigStatus status)
    {
        Status = status;
    }

    // ⚠️ QUAN TRỌNG: Không có method UpdateCode() và UpdateSubCode() - Code và SubCode không được phép sửa
    public virtual void UpdateName(string name)
    {
        SetName(name);
    }

    public virtual void UpdateValue(string value)
    {
        SetValue(value);
    }

    public virtual void UpdateDescription(string? description)
    {
        SetDescription(description);
    }

    public virtual void UpdateStatus(AdminConfigStatus status)
    {
        SetStatus(status);
    }
}

