using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using Volo.Abp.Domain.Entities.Auditing;
using iOne.ResUoms;

namespace iOne.ResUomClasses;

[Table("res_uom_class")]
public class ResUomClass : FullAuditedAggregateRoot<Guid>
{
    [Required]
    [MaxLength(25)]
    public virtual string Code { get; private set; } = null!;

    [Required]
    [MaxLength(100)]
    public virtual string Name { get; private set; } = null!;

    [Required]
    public virtual ResUomClassStatus Status { get; private set; }

    // Navigation property for one-to-many relationship
    public virtual ICollection<ResUom> Uoms { get; private set; } = new List<ResUom>();

    protected ResUomClass()
    {
        // For ORM
    }

    public ResUomClass(Guid id, string code, string name, ResUomClassStatus status)
        : base(id)
    {
        SetCode(code);
        SetName(name);
        SetStatus(status);
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

    private void SetStatus(ResUomClassStatus status)
    {
        Status = status;
    }

    // ⚠️ QUAN TRỌNG: Không có method UpdateCode() - Code không được phép sửa
    public virtual void UpdateName(string name)
    {
        SetName(name);
    }

    public virtual void UpdateStatus(ResUomClassStatus status)
    {
        SetStatus(status);
    }
}

