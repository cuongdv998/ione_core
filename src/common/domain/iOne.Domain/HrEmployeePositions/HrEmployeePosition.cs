using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using Volo.Abp.Domain.Entities.Auditing;

namespace iOne.HrEmployeePositions;

[Table("hr_employee_position")]
public class HrEmployeePosition : FullAuditedAggregateRoot<Guid>
{
    [Required]
    [MaxLength(50)]
    public virtual string Code { get; private set; } = null!;

    [Required]
    [MaxLength(250)]
    public virtual string Name { get; private set; } = null!;

    [Required]
    public virtual HrEmployeePositionType Type { get; private set; }

    [Required]
    public virtual HrEmployeePositionStatus Status { get; private set; }

    protected HrEmployeePosition()
    {
        // For ORM
    }

    public HrEmployeePosition(Guid id, string code, string name, HrEmployeePositionType type, HrEmployeePositionStatus status)
        : base(id)
    {
        SetCode(code);
        SetName(name);
        SetType(type);
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

    private void SetType(HrEmployeePositionType type)
    {
        Type = type;
    }

    private void SetStatus(HrEmployeePositionStatus status)
    {
        Status = status;
    }

    public virtual void UpdateName(string name)
    {
        SetName(name);
    }

    public virtual void UpdateType(HrEmployeePositionType type)
    {
        SetType(type);
    }

    public virtual void UpdateStatus(HrEmployeePositionStatus status)
    {
        SetStatus(status);
    }
}

