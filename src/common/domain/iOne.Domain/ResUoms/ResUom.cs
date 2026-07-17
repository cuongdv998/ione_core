using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using Volo.Abp.Domain.Entities.Auditing;
using iOne.ResUomClasses;
using iOne.ResObjectTypeItems;

namespace iOne.ResUoms;

[Table("res_uom")]
public class ResUom : FullAuditedAggregateRoot<Guid>
{
    [Required]
    public virtual Guid ClassId { get; private set; }

    [Required]
    [MaxLength(25)]
    public virtual string Code { get; private set; } = null!;

    [Required]
    [MaxLength(100)]
    public virtual string Name { get; private set; } = null!;

    [Required]
    public virtual ResUomStatus Status { get; private set; }

    [Required]
    [Column(TypeName = "decimal(2,4)")]
    public virtual decimal Rounding { get; private set; }

    [Column(TypeName = "decimal(6,6)")]
    public virtual decimal? Factor { get; private set; }

    [Required]
    public virtual ResUomType Type { get; private set; }

    // Navigation property for many-to-one relationship
    public virtual ResUomClass? UomClass { get; private set; }

    // Navigation property for one-to-many relationship with ResObjectTypeItem
    public virtual ICollection<ResObjectTypeItem> ObjectTypeItems { get; private set; } = new List<ResObjectTypeItem>();

    protected ResUom()
    {
        // For ORM
    }

    public ResUom(
        Guid id,
        Guid classId,
        string code,
        string name,
        ResUomStatus status,
        decimal rounding,
        decimal? factor,
        ResUomType type)
        : base(id)
    {
        SetClassId(classId);
        SetCode(code);
        SetName(name);
        SetStatus(status);
        SetRounding(rounding);
        SetFactor(factor);
        SetType(type);
    }

    private void SetClassId(Guid classId)
    {
        if (classId == Guid.Empty)
        {
            throw new ArgumentException("ClassId cannot be empty.", nameof(classId));
        }

        ClassId = classId;
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

        if (name.Length > 100)
        {
            throw new ArgumentException("Name cannot exceed 100 characters.", nameof(name));
        }

        Name = name;
    }

    private void SetStatus(ResUomStatus status)
    {
        Status = status;
    }

    private void SetRounding(decimal rounding)
    {
        if (rounding < 0)
        {
            throw new ArgumentException("Rounding cannot be negative.", nameof(rounding));
        }

        Rounding = rounding;
    }

    private void SetFactor(decimal? factor)
    {
        if (factor.HasValue)
        {
            if (factor.Value < 0)
            {
                throw new ArgumentException("Factor cannot be negative.", nameof(factor));
            }

            if (factor.Value >= 1)
            {
                throw new ArgumentException("Factor must be less than 1. The database column type decimal(6,6) only allows values from 0.000000 to 0.999999.", nameof(factor));
            }
        }

        Factor = factor;
    }

    private void SetType(ResUomType type)
    {
        Type = type;
    }

    // ⚠️ QUAN TRỌNG: Không có method UpdateCode() - Code không được phép sửa
    // ⚠️ QUAN TRỌNG: Không có method UpdateClassId() - ClassId không được phép sửa
    public virtual void UpdateName(string name)
    {
        SetName(name);
    }

    public virtual void UpdateStatus(ResUomStatus status)
    {
        SetStatus(status);
    }

    public virtual void UpdateRounding(decimal rounding)
    {
        SetRounding(rounding);
    }

    public virtual void UpdateFactor(decimal? factor)
    {
        SetFactor(factor);
    }

    public virtual void UpdateType(ResUomType type)
    {
        SetType(type);
    }
}
