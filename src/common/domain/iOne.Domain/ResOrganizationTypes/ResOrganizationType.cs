using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using Volo.Abp.Domain.Entities.Auditing;

namespace iOne.ResOrganizationTypes;

[Table("res_organization_type")]
public class ResOrganizationType : FullAuditedAggregateRoot<Guid>
{
    [Required]
    [MaxLength(25)]
    public virtual string Code { get; private set; } = null!;

    [Required]
    [MaxLength(250)]
    public virtual string Name { get; private set; } = null!;

    [Required]
    public virtual ResOrganizationTypeStatus Status { get; private set; }

    [Required]
    public virtual OrganizationTypeType Type { get; private set; }

    protected ResOrganizationType()
    {
        // For ORM - default Type is TC (Tổ chức)
        Type = OrganizationTypeType.TC;
    }

    public ResOrganizationType(Guid id, string code, string name, ResOrganizationTypeStatus status, OrganizationTypeType type = OrganizationTypeType.TC)
        : base(id)
    {
        SetCode(code);
        SetName(name);
        SetStatus(status);
        SetType(type);
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

    private void SetStatus(ResOrganizationTypeStatus status)
    {
        Status = status;
    }

    private void SetType(OrganizationTypeType type)
    {
        Type = type;
    }

    // ⚠️ QUAN TRỌNG: Không có method UpdateCode() - Code không được phép sửa
    public virtual void UpdateName(string name)
    {
        SetName(name);
    }

    public virtual void UpdateStatus(ResOrganizationTypeStatus status)
    {
        SetStatus(status);
    }

    public virtual void UpdateType(OrganizationTypeType type)
    {
        SetType(type);
    }
}

