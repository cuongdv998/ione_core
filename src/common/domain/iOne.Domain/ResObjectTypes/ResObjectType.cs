using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using Volo.Abp.Domain.Entities.Auditing;
using iOne.ResRisks;
using iOne.ResDamageLevels;
using iOne.ResObjectTypeItems;
using iOne.Policies;

namespace iOne.ResObjectTypes;

[Table("res_object_type")]
public class ResObjectType : FullAuditedAggregateRoot<Guid>
{
    [Required]
    [MaxLength(50)]
    public virtual string Code { get; private set; } = null!;

    [Required]
    [MaxLength(250)]
    public virtual string Name { get; private set; } = null!;

    [MaxLength(50)]
    public virtual string? ObjectGroup { get; private set; }

    [MaxLength(500)]
    public virtual string? Description { get; private set; }

    [Required]
    public virtual ResObjectTypeStatus Status { get; private set; }

    // Navigation property for one-to-many relationship
    public virtual ICollection<ResRisk> Risks { get; private set; } = new List<ResRisk>();

    // Navigation property for one-to-many relationship
    public virtual ICollection<ResDamageLevel> DamageLevels { get; private set; } = new List<ResDamageLevel>();

    // Navigation property for one-to-many relationship with ResObjectTypeItem
    public virtual ICollection<ResObjectTypeItem> ObjectTypeItems { get; private set; } = new List<ResObjectTypeItem>();

    // Navigation property for one-to-many relationship with PolicyRiskObject
    public virtual ICollection<Policies.PolicyRiskObject> PolicyRiskObjects { get; set; } = new List<Policies.PolicyRiskObject>();

    protected ResObjectType()
    {
        // For ORM
    }

    public ResObjectType(Guid id, string code, string name, string? objectGroup, string? description, ResObjectTypeStatus status)
        : base(id)
    {
        SetCode(code);
        SetName(name);
        SetObjectGroup(objectGroup);
        SetDescription(description);
        SetStatus(status);
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

    private void SetObjectGroup(string? objectGroup)
    {
        if (objectGroup != null && objectGroup.Length > 50)
        {
            throw new ArgumentException("ObjectGroup cannot exceed 50 characters.", nameof(objectGroup));
        }

        ObjectGroup = objectGroup;
    }

    private void SetDescription(string? description)
    {
        if (description != null && description.Length > 500)
        {
            throw new ArgumentException("Description cannot exceed 500 characters.", nameof(description));
        }

        Description = description;
    }

    private void SetStatus(ResObjectTypeStatus status)
    {
        Status = status;
    }

    // ⚠️ QUAN TRỌNG: Không có method UpdateCode() - Code không được phép sửa
    public virtual void UpdateName(string name)
    {
        SetName(name);
    }

    public virtual void UpdateObjectGroup(string? objectGroup)
    {
        SetObjectGroup(objectGroup);
    }

    public virtual void UpdateDescription(string? description)
    {
        SetDescription(description);
    }

    public virtual void UpdateStatus(ResObjectTypeStatus status)
    {
        SetStatus(status);
    }
}

