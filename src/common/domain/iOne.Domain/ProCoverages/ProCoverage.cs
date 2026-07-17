using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using Volo.Abp.Domain.Entities.Auditing;
using iOne.ProLineOfBusinesses;
using iOne.ResObjectTypes;
using iOne.ProCoverageGroups;
using iOne.ProCoverageTypes;
using iOne.Policies;

namespace iOne.ProCoverages;

[Table("pro_coverage")]
public class ProCoverage : FullAuditedAggregateRoot<Guid>
{
    [Required]
    public virtual Guid LobId { get; private set; }

    public virtual Guid? ObjectTypeId { get; private set; }

    [Required]
    public virtual Guid CoverageGroupId { get; private set; }

    public virtual Guid? CoverageTypeId { get; private set; }

    [Required]
    [MaxLength(50)]
    public virtual string Code { get; private set; } = null!;

    [Required]
    [MaxLength(50)]
    public virtual string ShortName { get; private set; } = null!;

    [Required]
    [MaxLength(250)]
    public virtual string Name { get; private set; } = null!;

    [Required]
    public virtual ProCoverageTermType Type { get; private set; }

    [MaxLength(500)]
    public virtual string? Description { get; private set; }

    [Required]
    public virtual ProCoverageStatus Status { get; private set; }

    // Navigation properties for EF Core
    public virtual ProLineOfBusiness? Lob { get; set; }
    public virtual ResObjectType? ObjectType { get; set; }
    public virtual ProCoverageGroup? CoverageGroup { get; set; }
    public virtual ProCoverageType? CoverageType { get; set; }
    public virtual ICollection<ProTableRateLines.ProTableRateLine> TableRateLines { get; set; } = new List<ProTableRateLines.ProTableRateLine>();
    public virtual ICollection<Policies.PolicyCoverage> PolicyCoverages { get; set; } = new List<Policies.PolicyCoverage>();
    public virtual ICollection<ProProducts.ProProductCoverage> ProductCoverages { get; set; } = new List<ProProducts.ProProductCoverage>();

    protected ProCoverage()
    {
        // For ORM
    }

    public ProCoverage(
        Guid id,
        Guid lobId,
        Guid coverageGroupId,
        ProCoverageTermType type,
        string code,
        string shortName,
        string name,
        ProCoverageStatus status,
        Guid? objectTypeId = null,
        Guid? coverageTypeId = null,
        string? description = null)
        : base(id)
    {
        SetLobId(lobId);
        SetCoverageGroupId(coverageGroupId);
        SetType(type);
        SetCode(code);
        SetShortName(shortName);
        SetName(name);
        SetStatus(status);
        SetObjectTypeId(objectTypeId);
        SetCoverageTypeId(coverageTypeId);
        SetDescription(description);
    }

    private void SetLobId(Guid lobId)
    {
        if (lobId == Guid.Empty)
        {
            throw new ArgumentException("LobId cannot be empty.", nameof(lobId));
        }

        LobId = lobId;
    }

    private void SetCoverageGroupId(Guid coverageGroupId)
    {
        if (coverageGroupId == Guid.Empty)
        {
            throw new ArgumentException("CoverageGroupId cannot be empty.", nameof(coverageGroupId));
        }

        CoverageGroupId = coverageGroupId;
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

        // Validate code format: only A-Z, 0-9, and underscore
        if (!Regex.IsMatch(code, @"^[A-Z0-9_]+$"))
        {
            throw new ArgumentException("Code can only contain uppercase letters (A-Z), numbers (0-9), and underscore (_).", nameof(code));
        }

        Code = code;
    }

    private void SetShortName(string shortName)
    {
        if (string.IsNullOrWhiteSpace(shortName))
        {
            throw new ArgumentException("ShortName cannot be null or empty.", nameof(shortName));
        }

        if (shortName.Length > 50)
        {
            throw new ArgumentException("ShortName cannot exceed 50 characters.", nameof(shortName));
        }

        ShortName = shortName;
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

    private void SetType(ProCoverageTermType type)
    {
        Type = type;
    }

    private void SetStatus(ProCoverageStatus status)
    {
        Status = status;
    }

    private void SetObjectTypeId(Guid? objectTypeId)
    {
        if (objectTypeId.HasValue && objectTypeId.Value == Guid.Empty)
        {
            throw new ArgumentException("ObjectTypeId cannot be empty if provided.", nameof(objectTypeId));
        }

        ObjectTypeId = objectTypeId;
    }

    private void SetCoverageTypeId(Guid? coverageTypeId)
    {
        if (coverageTypeId.HasValue && coverageTypeId.Value == Guid.Empty)
        {
            throw new ArgumentException("CoverageTypeId cannot be empty if provided.", nameof(coverageTypeId));
        }

        CoverageTypeId = coverageTypeId;
    }

    private void SetDescription(string? description)
    {
        if (description != null && description.Length > 500)
        {
            throw new ArgumentException("Description cannot exceed 500 characters.", nameof(description));
        }

        Description = description;
    }

    // ⚠️ QUAN TRỌNG: Không có method UpdateCode() - Code không được phép sửa
    public virtual void UpdateLobId(Guid lobId)
    {
        SetLobId(lobId);
    }

    public virtual void UpdateCoverageGroupId(Guid coverageGroupId)
    {
        SetCoverageGroupId(coverageGroupId);
    }

    public virtual void UpdateShortName(string shortName)
    {
        SetShortName(shortName);
    }

    public virtual void UpdateName(string name)
    {
        SetName(name);
    }

    public virtual void UpdateType(ProCoverageTermType type)
    {
        SetType(type);
    }

    public virtual void UpdateStatus(ProCoverageStatus status)
    {
        SetStatus(status);
    }

    public virtual void UpdateObjectTypeId(Guid? objectTypeId)
    {
        SetObjectTypeId(objectTypeId);
    }

    public virtual void UpdateCoverageTypeId(Guid? coverageTypeId)
    {
        SetCoverageTypeId(coverageTypeId);
    }

    public virtual void UpdateDescription(string? description)
    {
        SetDescription(description);
    }
}
