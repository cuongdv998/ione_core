using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using iOne.ProAttributes;
using iOne.ProTableRates;
using Volo.Abp.Domain.Entities.Auditing;

namespace iOne.ProTableRateVariables;

[Table("pro_table_rate_variable")]
public class ProTableRateVariable : AuditedEntity<Guid>
{
    [Required]
    public virtual Guid TableRateId { get; private set; }

    [Required]
    public virtual Guid AttributeId { get; private set; }

    [Required]
    [MaxLength(15)]
    public virtual string Operator { get; private set; } = null!;

    // Navigation properties
    public virtual ProTableRate? TableRate { get; set; }
    public virtual ProAttribute? Attribute { get; set; }

    protected ProTableRateVariable()
    {
        // For ORM
    }

    public ProTableRateVariable(
        Guid id,
        Guid tableRateId,
        Guid attributeId,
        string @operator)
        : base(id)
    {
        SetTableRateId(tableRateId);
        SetAttributeId(attributeId);
        SetOperator(@operator);
    }

    private void SetTableRateId(Guid tableRateId)
    {
        if (tableRateId == Guid.Empty)
        {
            throw new ArgumentException("TableRateId cannot be empty.", nameof(tableRateId));
        }

        TableRateId = tableRateId;
    }

    private void SetAttributeId(Guid attributeId)
    {
        if (attributeId == Guid.Empty)
        {
            throw new ArgumentException("AttributeId cannot be empty.", nameof(attributeId));
        }

        AttributeId = attributeId;
    }

    private void SetOperator(string @operator)
    {
        if (string.IsNullOrWhiteSpace(@operator))
        {
            throw new ArgumentException("Operator cannot be null or empty.", nameof(@operator));
        }

        if (@operator.Length > 15)
        {
            throw new ArgumentException("Operator cannot exceed 15 characters.", nameof(@operator));
        }

        Operator = @operator;
    }

    // Note: UpdateTableRateId is not needed as it's part of the aggregate root
    // Variables should be managed through ProTableRate aggregate

    public virtual void UpdateAttributeId(Guid attributeId)
    {
        SetAttributeId(attributeId);
    }

    public virtual void UpdateOperator(string @operator)
    {
        SetOperator(@operator);
    }
}
