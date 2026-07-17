using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using Volo.Abp.Domain.Entities.Auditing;

namespace iOne.ResBusinessAuthorities;

[Table("res_business_authority")]
public class ResBusinessAuthority : FullAuditedAggregateRoot<Guid>
{
    [Required]
    [MaxLength(50)]
    public virtual string BusinessCode { get; private set; } = null!;

    [Required]
    [MaxLength(25)]
    public virtual string Code { get; private set; } = null!;

    [Required]
    [MaxLength(250)]
    public virtual string Name { get; private set; } = null!;

    [Required]
    public virtual ResBusinessAuthorityStatus Status { get; private set; }

    protected ResBusinessAuthority()
    {
        // For ORM
    }

    public ResBusinessAuthority(
        Guid id,
        string businessCode,
        string code,
        string name,
        ResBusinessAuthorityStatus status)
        : base(id)
    {
        SetBusinessCode(businessCode);
        SetCode(code);
        SetName(name);
        SetStatus(status);
    }

    private void SetBusinessCode(string businessCode)
    {
        if (string.IsNullOrWhiteSpace(businessCode))
        {
            throw new ArgumentException("BusinessCode cannot be null or empty.", nameof(businessCode));
        }

        if (businessCode.Length > 50)
        {
            throw new ArgumentException("BusinessCode cannot exceed 50 characters.", nameof(businessCode));
        }

        BusinessCode = businessCode;
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

    private void SetStatus(ResBusinessAuthorityStatus status)
    {
        Status = status;
    }

    public virtual void UpdateName(string name)
    {
        SetName(name);
    }

    public virtual void UpdateStatus(ResBusinessAuthorityStatus status)
    {
        SetStatus(status);
    }
}
