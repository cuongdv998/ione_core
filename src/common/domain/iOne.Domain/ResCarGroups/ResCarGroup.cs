using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using Volo.Abp.Domain.Entities.Auditing;
using iOne.ResCarLines;
using iOne.ResObjectItemDepreciations;

namespace iOne.ResCarGroups;

[Table("res_car_group")]
public class ResCarGroup : FullAuditedAggregateRoot<Guid>
{
    public virtual Guid? CarLineId { get; private set; }

    [Required]
    [MaxLength(50)]
    public virtual string Code { get; private set; } = null!;

    [Required]
    [MaxLength(250)]
    public virtual string Name { get; private set; } = null!;

    [MaxLength(500)]
    public virtual string? Description { get; private set; }

    [Required]
    public virtual ResCarGroupStatus Status { get; private set; }

    // Navigation property for many-to-one relationship
    public virtual ResCarLine? CarLine { get; private set; }

    // Navigation property for one-to-many relationship
    public virtual ICollection<ResObjectItemDepreciation> ObjectItemDepreciations { get; private set; } = new List<ResObjectItemDepreciation>();

    protected ResCarGroup()
    {
        // For ORM
    }

    public ResCarGroup(Guid id, Guid? carLineId, string code, string name, string? description, ResCarGroupStatus status)
        : base(id)
    {
        SetCarLineId(carLineId);
        SetCode(code);
        SetName(name);
        SetDescription(description);
        SetStatus(status);
    }

    private void SetCarLineId(Guid? carLineId)
    {
        CarLineId = carLineId;
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

    private void SetStatus(ResCarGroupStatus status)
    {
        Status = status;
    }

    // ⚠️ QUAN TRỌNG: Không có method UpdateCode() - Code không được phép sửa
    public virtual void UpdateCarLineId(Guid? carLineId)
    {
        SetCarLineId(carLineId);
    }

    public virtual void UpdateName(string name)
    {
        SetName(name);
    }

    public virtual void UpdateDescription(string? description)
    {
        SetDescription(description);
    }

    public virtual void UpdateStatus(ResCarGroupStatus status)
    {
        SetStatus(status);
    }
}
