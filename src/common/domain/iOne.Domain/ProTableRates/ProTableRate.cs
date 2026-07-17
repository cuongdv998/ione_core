using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using iOne.ProLineOfBusinesses;
using iOne.ResPartners;
using Volo.Abp.Domain.Entities.Auditing;

namespace iOne.ProTableRates;

[Table("pro_table_rate")]
public class ProTableRate : FullAuditedAggregateRoot<Guid>
{
    [Required]
    public virtual Guid LobId { get; private set; }

    public virtual Guid? InsurerId { get; private set; }

    [Required]
    [MaxLength(50)]
    public virtual string Code { get; private set; } = null!;

    [Required]
    [MaxLength(250)]
    public virtual string Name { get; private set; } = null!;

    [MaxLength(500)]
    public virtual string? Description { get; private set; }

    [Required]
    public virtual ProTableRateStatus Status { get; private set; }

    // Navigation properties
    public virtual ProLineOfBusiness? Lob { get; set; }
    public virtual ResPartner? Insurer { get; set; }
    public virtual ICollection<ProTableRateVariables.ProTableRateVariable> Variables { get; set; } = new List<ProTableRateVariables.ProTableRateVariable>();
    public virtual ICollection<ProTableRateLines.ProTableRateLine> Lines { get; set; } = new List<ProTableRateLines.ProTableRateLine>();

    protected ProTableRate()
    {
        // For ORM
    }

    public ProTableRate(
        Guid id,
        Guid lobId,
        string code,
        string name,
        ProTableRateStatus status,
        Guid? insurerId = null,
        string? description = null)
        : base(id)
    {
        SetLobId(lobId);
        SetCode(code);
        SetName(name);
        SetStatus(status);
        SetInsurerId(insurerId);
        SetDescription(description);
    }

    private void SetLobId(Guid lobId)
    {
        if (lobId == Guid.Empty)
        {
            throw new ArgumentException("LobId cannot be empty.", nameof(lobId));
        }

        LobId = lobId;
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

    private void SetStatus(ProTableRateStatus status)
    {
        Status = status;
    }

    private void SetInsurerId(Guid? insurerId)
    {
        if (insurerId.HasValue && insurerId.Value == Guid.Empty)
        {
            throw new ArgumentException("InsurerId cannot be empty Guid.", nameof(insurerId));
        }

        InsurerId = insurerId;
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

    public virtual void UpdateStatus(ProTableRateStatus status)
    {
        SetStatus(status);
    }

    public virtual void UpdateDescription(string? description)
    {
        SetDescription(description);
    }

    public virtual void UpdateLobId(Guid lobId)
    {
        SetLobId(lobId);
    }

    public virtual void UpdateInsurerId(Guid? insurerId)
    {
        SetInsurerId(insurerId);
    }

    // Methods to manage Variables collection
    public virtual void AddVariable(ProTableRateVariables.ProTableRateVariable variable)
    {
        if (variable == null)
        {
            throw new ArgumentNullException(nameof(variable));
        }

        if (variable.TableRateId != Id)
        {
            throw new ArgumentException("Variable.TableRateId must match this TableRate.Id", nameof(variable));
        }

        Variables.Add(variable);
    }

    public virtual void RemoveVariable(ProTableRateVariables.ProTableRateVariable variable)
    {
        if (variable == null)
        {
            throw new ArgumentNullException(nameof(variable));
        }

        Variables.Remove(variable);
    }
}
