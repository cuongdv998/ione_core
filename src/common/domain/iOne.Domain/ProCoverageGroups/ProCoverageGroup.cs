using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using Volo.Abp.Domain.Entities.Auditing;

namespace iOne.ProCoverageGroups;

[Table("pro_coverage_group")]
public class ProCoverageGroup : FullAuditedAggregateRoot<Guid>
{
    [Required]
    [MaxLength(50)]
    public virtual string Code { get; private set; } = null!;

    [Required]
    [MaxLength(250)]
    public virtual string Name { get; private set; } = null!;

    [MaxLength(500)]
    public virtual string? Description { get; private set; }

    [Required]
    public virtual ProCoverageGroupStatus Status { get; private set; }

    protected ProCoverageGroup()
    {
        // For ORM
    }

    public ProCoverageGroup(Guid id, string code, string name, ProCoverageGroupStatus status, string? description = null)
        : base(id)
    {
        SetCode(code);
        SetName(name);
        SetStatus(status);
        SetDescription(description);
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

    private void SetStatus(ProCoverageGroupStatus status)
    {
        Status = status;
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
    public virtual void UpdateName(string name)
    {
        SetName(name);
    }

    public virtual void UpdateStatus(ProCoverageGroupStatus status)
    {
        SetStatus(status);
    }

    public virtual void UpdateDescription(string? description)
    {
        SetDescription(description);
    }
}

