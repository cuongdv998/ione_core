using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using Volo.Abp.Domain.Entities.Auditing;
using iOne.ResObjectTypes;

namespace iOne.ResRisks;

[Table("res_risk")]
public class ResRisk : FullAuditedAggregateRoot<Guid>
{
    [Required]
    public virtual Guid ObjectTypeId { get; private set; }

    [Required]
    [MaxLength(50)]
    public virtual string Code { get; private set; } = null!;

    [Required]
    [MaxLength(250)]
    public virtual string Name { get; private set; } = null!;

    [MaxLength(500)]
    public virtual string? Description { get; private set; }

    [Required]
    public virtual ResRiskStatus Status { get; private set; }

    // Navigation property for many-to-one relationship
    public virtual ResObjectType ObjectType { get; private set; } = null!;

    protected ResRisk()
    {
        // For ORM
    }

    public ResRisk(Guid id, Guid objectTypeId, string code, string name, string? description, ResRiskStatus status)
        : base(id)
    {
        SetObjectTypeId(objectTypeId);
        SetCode(code);
        SetName(name);
        SetDescription(description);
        SetStatus(status);
    }

    private void SetObjectTypeId(Guid objectTypeId)
    {
        if (objectTypeId == Guid.Empty)
        {
            throw new ArgumentException("ObjectTypeId cannot be empty.", nameof(objectTypeId));
        }

        ObjectTypeId = objectTypeId;
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

    private void SetStatus(ResRiskStatus status)
    {
        Status = status;
    }

    // ⚠️ QUAN TRỌNG: Không có method UpdateCode() - Code không được phép sửa
    public virtual void UpdateObjectTypeId(Guid objectTypeId)
    {
        SetObjectTypeId(objectTypeId);
    }

    public virtual void UpdateName(string name)
    {
        SetName(name);
    }

    public virtual void UpdateDescription(string? description)
    {
        SetDescription(description);
    }

    public virtual void UpdateStatus(ResRiskStatus status)
    {
        SetStatus(status);
    }
}

