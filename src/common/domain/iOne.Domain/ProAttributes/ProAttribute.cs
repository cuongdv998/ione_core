using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using Volo.Abp.Domain.Entities.Auditing;

namespace iOne.ProAttributes;

[Table("pro_attribute")]
public class ProAttribute : FullAuditedAggregateRoot<Guid>
{
    [Required]
    [MaxLength(50)]
    public virtual string Code { get; private set; } = null!;

    [Required]
    [MaxLength(250)]
    public virtual string Name { get; private set; } = null!;

    [Required]
    public virtual ProAttributeStatus Status { get; private set; }

    [Required]
    public virtual ProAttributeSpec Spec { get; private set; }

    [MaxLength(250)]
    public virtual string? Description { get; private set; }

    [MaxLength(50)]
    public virtual string? DataPath { get; private set; }

    [Required]
    public virtual ProAttributeDataType DataType { get; private set; }

    public virtual string? ComputeScript { get; private set; }

    [MaxLength(1000)]
    public virtual string? ClearDataScript { get; private set; }

    // Navigation properties
    public virtual ICollection<ProTableRateVariables.ProTableRateVariable> TableRateVariables { get; set; } = new List<ProTableRateVariables.ProTableRateVariable>();
    
    // Navigation property for many-to-many relationship with ProProduct
    public virtual ICollection<ProProducts.ProProductAttribute> ProductAttributes { get; private set; } = new List<ProProducts.ProProductAttribute>();

    protected ProAttribute()
    {
        // For ORM
    }

    public ProAttribute(
        Guid id,
        string code,
        string name,
        ProAttributeStatus status,
        ProAttributeSpec spec,
        ProAttributeDataType dataType,
        string? description = null,
        string? dataPath = null,
        string? computeScript = null,
        string? clearDataScript = null)
        : base(id)
    {
        SetCode(code);
        SetName(name);
        SetStatus(status);
        SetSpec(spec);
        SetDataType(dataType);
        SetDescription(description);
        SetDataPath(dataPath);
        SetComputeScript(computeScript);
        SetClearDataScript(clearDataScript);
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

        // Validate code format: only A-Z, a-z, 0-9, and underscore
        if (!Regex.IsMatch(code, @"^[A-Za-z0-9_]+$"))
        {
            throw new ArgumentException("Code can only contain letters (A-Z, a-z), numbers (0-9), and underscore (_).", nameof(code));
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

    private void SetStatus(ProAttributeStatus status)
    {
        Status = status;
    }

    private void SetSpec(ProAttributeSpec spec)
    {
        Spec = spec;
    }

    private void SetDataType(ProAttributeDataType dataType)
    {
        DataType = dataType;
    }

    private void SetDescription(string? description)
    {
        if (description != null && description.Length > 250)
        {
            throw new ArgumentException("Description cannot exceed 250 characters.", nameof(description));
        }

        Description = description;
    }

    private void SetDataPath(string? dataPath)
    {
        if (dataPath != null && dataPath.Length > 50)
        {
            throw new ArgumentException("DataPath cannot exceed 50 characters.", nameof(dataPath));
        }

        DataPath = dataPath;
    }

    private void SetComputeScript(string? computeScript)
    {
        ComputeScript = computeScript;
    }

    private void SetClearDataScript(string? clearDataScript)
    {
        if (clearDataScript != null && clearDataScript.Length > 1000)
        {
            throw new ArgumentException("ClearDataScript cannot exceed 1000 characters.", nameof(clearDataScript));
        }

        ClearDataScript = clearDataScript;
    }

    // ⚠️ QUAN TRỌNG: Không có method UpdateCode() - Code không được phép sửa
    public virtual void UpdateName(string name)
    {
        SetName(name);
    }

    public virtual void UpdateStatus(ProAttributeStatus status)
    {
        SetStatus(status);
    }

    public virtual void UpdateSpec(ProAttributeSpec spec)
    {
        SetSpec(spec);
    }

    public virtual void UpdateDataType(ProAttributeDataType dataType)
    {
        SetDataType(dataType);
    }

    public virtual void UpdateDescription(string? description)
    {
        SetDescription(description);
    }

    public virtual void UpdateDataPath(string? dataPath)
    {
        SetDataPath(dataPath);
    }

    public virtual void UpdateComputeScript(string? computeScript)
    {
        SetComputeScript(computeScript);
    }

    public virtual void UpdateClearDataScript(string? clearDataScript)
    {
        SetClearDataScript(clearDataScript);
    }
}
