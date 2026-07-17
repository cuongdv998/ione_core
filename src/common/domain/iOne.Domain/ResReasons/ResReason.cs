using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using Volo.Abp.Domain.Entities.Auditing;
using iOne.ResReasonGroups;

namespace iOne.ResReasons;

[Table("res_reason")]
public class ResReason : FullAuditedAggregateRoot<Guid>
{
    [Required]
    public virtual Guid GroupId { get; private set; }

    [Required]
    [MaxLength(50)]
    public virtual string Code { get; private set; } = null!;

    [Required]
    [MaxLength(250)]
    public virtual string Name { get; private set; } = null!;

    [MaxLength(500)]
    public virtual string? Description { get; private set; }

    [Required]
    public virtual ResReasonGroupStatus Status { get; private set; }

    // Navigation property
    public virtual ResReasonGroup ReasonGroup { get; private set; } = null!;

    protected ResReason()
    {
        // For ORM
    }

    public ResReason(Guid id, Guid groupId, string code, string name, string? description, ResReasonGroupStatus status)
        : base(id)
    {
        SetGroupId(groupId);
        SetCode(code);
        SetName(name);
        SetDescription(description);
        SetStatus(status);
    }

    private void SetGroupId(Guid groupId)
    {
        if (groupId == Guid.Empty)
        {
            throw new ArgumentException("GroupId cannot be empty.", nameof(groupId));
        }

        GroupId = groupId;
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

    private void SetStatus(ResReasonGroupStatus status)
    {
        Status = status;
    }

    // ⚠️ QUAN TRỌNG: Không có method UpdateCode() - Code không được phép sửa
    // ⚠️ QUAN TRỌNG: Không có method UpdateGroupId() - GroupId không được phép sửa
    public virtual void UpdateName(string name)
    {
        SetName(name);
    }

    public virtual void UpdateDescription(string? description)
    {
        SetDescription(description);
    }

    public virtual void UpdateStatus(ResReasonGroupStatus status)
    {
        SetStatus(status);
    }
}
