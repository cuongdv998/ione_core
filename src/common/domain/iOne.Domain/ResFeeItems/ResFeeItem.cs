using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using Volo.Abp.Domain.Entities.Auditing;
using iOne.ResTaxes;

namespace iOne.ResFeeItems;

[Table("res_fee_item")]
public class ResFeeItem : FullAuditedAggregateRoot<Guid>
{
    [Required]
    [MaxLength(50)]
    public virtual string Code { get; private set; } = null!;

    [Required]
    [MaxLength(250)]
    public virtual string Name { get; private set; } = null!;

    [MaxLength(36)]
    public virtual Guid? TaxId { get; private set; }

    [MaxLength(500)]
    public virtual string? Description { get; private set; }

    [Required]
    public virtual ResFeeItemStatus Status { get; private set; }

    // Navigation property for many-to-one relationship with ResTax
    public virtual ResTax? Tax { get; private set; }

    protected ResFeeItem()
    {
        // For ORM
    }

    public ResFeeItem(Guid id, string code, string name, string? description, ResFeeItemStatus status, Guid? taxId = null)
        : base(id)
    {
        SetCode(code);
        SetName(name);
        SetDescription(description);
        SetStatus(status);
        SetTaxId(taxId);
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

    private void SetStatus(ResFeeItemStatus status)
    {
        Status = status;
    }

    private void SetTaxId(Guid? taxId)
    {
        TaxId = taxId;
    }

    // ⚠️ QUAN TRỌNG: Không có method UpdateCode() - Code không được phép sửa
    public virtual void UpdateName(string name)
    {
        SetName(name);
    }

    public virtual void UpdateDescription(string? description)
    {
        SetDescription(description);
    }

    public virtual void UpdateStatus(ResFeeItemStatus status)
    {
        SetStatus(status);
    }

    public virtual void UpdateTaxId(Guid? taxId)
    {
        SetTaxId(taxId);
    }
}
