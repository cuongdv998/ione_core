using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using Volo.Abp.Domain.Entities.Auditing;

namespace iOne.ResTaskCategories;

[Table("res_task_category")]
public class ResTaskCategory : FullAuditedAggregateRoot<Guid>
{
    [Required]
    public virtual ResTaskCategoryBusinessType BusinessType { get; private set; }

    [Required]
    [MaxLength(50)]
    public virtual string Code { get; private set; } = null!;

    [Required]
    [MaxLength(250)]
    public virtual string Name { get; private set; } = null!;

    [Required]
    public virtual ResTaskCategoryStatus Status { get; private set; }

    protected ResTaskCategory()
    {
        // For ORM
    }

    public ResTaskCategory(
        Guid id,
        ResTaskCategoryBusinessType businessType,
        string code,
        string name,
        ResTaskCategoryStatus status)
        : base(id)
    {
        SetBusinessType(businessType);
        SetCode(code);
        SetName(name);
        SetStatus(status);
    }

    private void SetBusinessType(ResTaskCategoryBusinessType businessType)
    {
        BusinessType = businessType;
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

    private void SetStatus(ResTaskCategoryStatus status)
    {
        Status = status;
    }

    public virtual void UpdateName(string name)
    {
        SetName(name);
    }

    public virtual void UpdateStatus(ResTaskCategoryStatus status)
    {
        SetStatus(status);
    }
}
