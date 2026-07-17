using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using Volo.Abp.Domain.Entities.Auditing;
using iOne.ResProvinces;

namespace iOne.ResWards;

[Table("res_ward")]
public class ResWard : FullAuditedAggregateRoot<Guid>
{
    [Required]
    public virtual Guid ProvinceId { get; private set; }

    [Required]
    [MaxLength(25)]
    public virtual string Code { get; private set; } = null!;

    [Required]
    [MaxLength(250)]
    public virtual string Name { get; private set; } = null!;

    [Required]
    public virtual ResWardStatus Status { get; private set; }

    [MaxLength(500)]
    public virtual string? Description { get; private set; }

    // Navigation property
    public virtual ResProvince? Province { get; set; }

    protected ResWard()
    {
        // For ORM
    }

    public ResWard(
        Guid id,
        Guid provinceId,
        string code,
        string name,
        ResWardStatus status,
        string? description = null)
        : base(id)
    {
        SetProvinceId(provinceId);
        SetCode(code);
        SetName(name);
        SetStatus(status);
        SetDescription(description);
    }

    private void SetProvinceId(Guid provinceId)
    {
        if (provinceId == Guid.Empty)
        {
            throw new ArgumentException("ProvinceId cannot be empty.", nameof(provinceId));
        }

        ProvinceId = provinceId;
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

    private void SetStatus(ResWardStatus status)
    {
        Status = status;
    }

    private void SetDescription(string? description)
    {
        if (!string.IsNullOrWhiteSpace(description) && description.Length > 500)
        {
            throw new ArgumentException("Description cannot exceed 500 characters.", nameof(description));
        }

        Description = description;
    }

    // ⚠️ QUAN TRỌNG: Không có method UpdateCode() - Code không được phép sửa
    public virtual void UpdateProvinceId(Guid provinceId)
    {
        SetProvinceId(provinceId);
    }

    public virtual void UpdateName(string name)
    {
        SetName(name);
    }

    public virtual void UpdateStatus(ResWardStatus status)
    {
        SetStatus(status);
    }

    public virtual void UpdateDescription(string? description)
    {
        SetDescription(description);
    }
}

