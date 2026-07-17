using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using Volo.Abp.Domain.Entities.Auditing;
using iOne.ResObjectTypes;
using iOne.ResObjectItemTypes;
using iOne.ResUoms;
using iOne.ResObjectItemDepreciations;

namespace iOne.ResObjectTypeItems;

[Table("res_object_type_item")]
public class ResObjectTypeItem : FullAuditedAggregateRoot<Guid>
{
    [Required]
    public virtual Guid ObjectTypeId { get; private set; }

    public virtual Guid? ObjectItemType { get; private set; }

    [Required]
    [MaxLength(50)]
    public virtual string Code { get; private set; } = null!;

    [Required]
    [MaxLength(250)]
    public virtual string Name { get; private set; } = null!;

    [Required]
    public virtual Guid UomId { get; private set; }

    [MaxLength(500)]
    public virtual string? Description { get; private set; }

    [Required]
    public virtual ResObjectTypeItemStatus Status { get; private set; }

    // Navigation properties for many-to-one relationships
    public virtual ResObjectType? ObjectTypeNavigation { get; private set; }
    public virtual ResObjectItemType? ObjectItemTypeNavigation { get; private set; }
    public virtual ResUom? Uom { get; private set; }

    // Navigation property for one-to-many relationship with ResObjectItemDepreciation
    public virtual ICollection<ResObjectItemDepreciation> ObjectItemDepreciations { get; private set; } = new List<ResObjectItemDepreciation>();

    protected ResObjectTypeItem()
    {
        // For ORM
    }

    public ResObjectTypeItem(
        Guid id, 
        Guid objectTypeId, 
        Guid? objectItemType, 
        string code, 
        string name, 
        Guid uomId, 
        string? description, 
        ResObjectTypeItemStatus status)
        : base(id)
    {
        SetObjectTypeId(objectTypeId);
        SetObjectItemType(objectItemType);
        SetCode(code);
        SetName(name);
        SetUomId(uomId);
        SetDescription(description);
        SetStatus(status);
    }

    private void SetObjectTypeId(Guid objectTypeId)
    {
        if (objectTypeId == Guid.Empty)
        {
            throw new ArgumentException("ObjectTypeId cannot be empty.", nameof(objectTypeId));
        }

        ObjectTypeId = objectTypeId;
    }

    private void SetObjectItemType(Guid? objectItemType)
    {
        ObjectItemType = objectItemType;
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

    private void SetUomId(Guid uomId)
    {
        if (uomId == Guid.Empty)
        {
            throw new ArgumentException("UomId cannot be empty.", nameof(uomId));
        }

        UomId = uomId;
    }

    private void SetDescription(string? description)
    {
        if (description != null && description.Length > 500)
        {
            throw new ArgumentException("Description cannot exceed 500 characters.", nameof(description));
        }

        Description = description;
    }

    private void SetStatus(ResObjectTypeItemStatus status)
    {
        Status = status;
    }

    // ⚠️ QUAN TRỌNG: Không có method UpdateCode() - Code không được phép sửa
    public virtual void UpdateObjectTypeId(Guid objectTypeId)
    {
        SetObjectTypeId(objectTypeId);
    }

    public virtual void UpdateObjectItemType(Guid? objectItemType)
    {
        SetObjectItemType(objectItemType);
    }

    public virtual void UpdateName(string name)
    {
        SetName(name);
    }

    public virtual void UpdateUomId(Guid uomId)
    {
        SetUomId(uomId);
    }

    public virtual void UpdateDescription(string? description)
    {
        SetDescription(description);
    }

    public virtual void UpdateStatus(ResObjectTypeItemStatus status)
    {
        SetStatus(status);
    }
}
