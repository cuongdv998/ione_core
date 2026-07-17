using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using iOne.HrEmployees;
using iOne.ProLineOfBusinesses;
using iOne.ResCustomers;
using iOne.ResPartners;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;

namespace iOne.PolicyContracts;

[Table("policy_contract")]
public class PolicyContract : FullAuditedAggregateRoot<Guid>, IEntity<Guid>
{
    // Basic Info
    public virtual Guid? InsurerId { get; private set; }

    [MaxLength(50)]
    public virtual string? InsurerContractCode { get; private set; }

    public virtual Guid? LobId { get; private set; }

    [Required]
    [MaxLength(50)]
    public virtual string Code { get; private set; } = null!;

    [Required]
    [MaxLength(250)]
    public virtual string Name { get; private set; } = null!;

    [Required]
    public virtual PolicyContractType Type { get; private set; }

    [Required]
    public virtual Guid CustomerId { get; private set; }

    // Payer Info
    [MaxLength(250)]
    public virtual string? PayerName { get; private set; }

    [MaxLength(50)]
    public virtual string? PayerEmail { get; private set; }

    [MaxLength(15)]
    public virtual string? PayerPhone { get; private set; }

    public virtual Guid? PayerProvinceId { get; private set; }

    public virtual Guid? PayerWardId { get; private set; }

    [MaxLength(250)]
    public virtual string? PayerAddress { get; private set; }

    [MaxLength(500)]
    public virtual string? PayerFullAddress { get; private set; }

    [MaxLength(50)]
    public virtual string? PayerTin { get; private set; }

    // Description
    [MaxLength(500)]
    public virtual string? Description { get; private set; }

    // Dates
    [Required]
    public virtual DateTime EffectDate { get; private set; }

    public virtual DateTime? ExpireDate { get; private set; }

    // Quantity
    [Required]
    [Column(TypeName = "NUMERIC(6)")]
    public virtual decimal Quantity { get; private set; }

    [Column(TypeName = "NUMERIC(6)")]
    public virtual decimal? CurrentQuantity { get; private set; }

    // Status
    [Required]
    public virtual PolicyContractStatus Status { get; private set; }

    // Employee
    public virtual Guid? EmployeeId { get; private set; }

    // Cancellation & Termination
    public virtual DateTime? CancellationDate { get; private set; }

    public virtual DateTime? TerminationDate { get; private set; }

    public virtual Guid? CancellationReasonId { get; private set; }

    public virtual Guid? TerminationReasonId { get; private set; }

    [MaxLength(500)]
    public virtual string? CancellationDescription { get; private set; }

    [MaxLength(500)]
    public virtual string? TerminationDescription { get; private set; }

    // Flags
    [Required]
    [MaxLength(1)]
    public virtual string IsReciveInvoice { get; private set; } = "N";

    // Quotation
    public virtual Guid? QuotationId { get; private set; }

    // Navigation Properties
    public virtual ResPartner? Insurer { get; set; }
    public virtual ResCustomer Customer { get; set; } = null!;
    public virtual ProLineOfBusiness? Lob { get; set; }
    public virtual HrEmployee? Employee { get; set; }
    public virtual ICollection<Policies.Policy> Policies { get; set; } = new List<Policies.Policy>();
    public virtual ICollection<PolicyContractDocument> Documents { get; set; } = new List<PolicyContractDocument>();

    protected PolicyContract()
    {
        // For ORM
    }

    public PolicyContract(
        Guid id,
        string code,
        string name,
        PolicyContractType type,
        Guid customerId,
        DateTime effectDate,
        decimal quantity,
        PolicyContractStatus status,
        Guid? insurerId = null,
        string? insurerContractCode = null,
        Guid? lobId = null,
        string? payerName = null,
        string? payerEmail = null,
        string? payerPhone = null,
        Guid? payerProvinceId = null,
        Guid? payerWardId = null,
        string? payerAddress = null,
        string? payerFullAddress = null,
        string? payerTin = null,
        string? description = null,
        DateTime? expireDate = null,
        decimal? currentQuantity = null,
        Guid? employeeId = null,
        DateTime? cancellationDate = null,
        DateTime? terminationDate = null,
        Guid? cancellationReasonId = null,
        Guid? terminationReasonId = null,
        string? cancellationDescription = null,
        string? terminationDescription = null,
        string isReciveInvoice = "N",
        Guid? quotationId = null)
        : base(id)
    {
        SetInsurerId(insurerId);
        SetInsurerContractCode(insurerContractCode);
        SetLobId(lobId);
        SetCode(code);
        SetName(name);
        SetType(type);
        SetCustomerId(customerId);
        SetPayerName(payerName);
        SetPayerEmail(payerEmail);
        SetPayerPhone(payerPhone);
        SetPayerProvinceId(payerProvinceId);
        SetPayerWardId(payerWardId);
        SetPayerAddress(payerAddress);
        SetPayerFullAddress(payerFullAddress);
        SetPayerTin(payerTin);
        SetDescription(description);
        SetEffectDate(effectDate);
        SetExpireDate(expireDate);
        SetQuantity(quantity);
        SetCurrentQuantity(currentQuantity);
        SetStatus(status);
        SetEmployeeId(employeeId);
        SetCancellationDate(cancellationDate);
        SetTerminationDate(terminationDate);
        SetCancellationReasonId(cancellationReasonId);
        SetTerminationReasonId(terminationReasonId);
        SetCancellationDescription(cancellationDescription);
        SetTerminationDescription(terminationDescription);
        SetIsReciveInvoice(isReciveInvoice);
        SetQuotationId(quotationId);
    }

    // Private setters with validation
    private void SetInsurerId(Guid? insurerId)
    {
        InsurerId = insurerId;
    }

    private void SetInsurerContractCode(string? insurerContractCode)
    {
        if (insurerContractCode != null && insurerContractCode.Length > 50)
        {
            throw new ArgumentException("InsurerContractCode cannot exceed 50 characters.", nameof(insurerContractCode));
        }
        InsurerContractCode = insurerContractCode;
    }

    private void SetLobId(Guid? lobId)
    {
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

    private void SetType(PolicyContractType type)
    {
        Type = type;
    }

    private void SetCustomerId(Guid customerId)
    {
        if (customerId == Guid.Empty)
        {
            throw new ArgumentException("CustomerId cannot be empty.", nameof(customerId));
        }
        CustomerId = customerId;
    }

    private void SetPayerName(string? payerName)
    {
        if (payerName != null && payerName.Length > 250)
        {
            throw new ArgumentException("PayerName cannot exceed 250 characters.", nameof(payerName));
        }
        PayerName = payerName;
    }

    private void SetPayerEmail(string? payerEmail)
    {
        if (payerEmail != null && payerEmail.Length > 50)
        {
            throw new ArgumentException("PayerEmail cannot exceed 50 characters.", nameof(payerEmail));
        }
        PayerEmail = payerEmail;
    }

    private void SetPayerPhone(string? payerPhone)
    {
        if (payerPhone != null && payerPhone.Length > 15)
        {
            throw new ArgumentException("PayerPhone cannot exceed 15 characters.", nameof(payerPhone));
        }
        PayerPhone = payerPhone;
    }

    private void SetPayerProvinceId(Guid? payerProvinceId)
    {
        PayerProvinceId = payerProvinceId;
    }

    private void SetPayerWardId(Guid? payerWardId)
    {
        PayerWardId = payerWardId;
    }

    private void SetPayerAddress(string? payerAddress)
    {
        if (payerAddress != null && payerAddress.Length > 250)
        {
            throw new ArgumentException("PayerAddress cannot exceed 250 characters.", nameof(payerAddress));
        }
        PayerAddress = payerAddress;
    }

    private void SetPayerFullAddress(string? payerFullAddress)
    {
        if (payerFullAddress != null && payerFullAddress.Length > 500)
        {
            throw new ArgumentException("PayerFullAddress cannot exceed 500 characters.", nameof(payerFullAddress));
        }
        PayerFullAddress = payerFullAddress;
    }

    private void SetPayerTin(string? payerTin)
    {
        if (payerTin != null && payerTin.Length > 50)
        {
            throw new ArgumentException("PayerTin cannot exceed 50 characters.", nameof(payerTin));
        }
        PayerTin = payerTin;
    }

    private void SetDescription(string? description)
    {
        if (description != null && description.Length > 500)
        {
            throw new ArgumentException("Description cannot exceed 500 characters.", nameof(description));
        }
        Description = description;
    }

    private void SetEffectDate(DateTime effectDate)
    {
        EffectDate = effectDate;
    }

    private void SetExpireDate(DateTime? expireDate)
    {
        if (expireDate.HasValue && expireDate.Value < EffectDate)
        {
            throw new ArgumentException("ExpireDate must be after EffectDate.", nameof(expireDate));
        }
        ExpireDate = expireDate;
    }

    private void SetQuantity(decimal quantity)
    {
        if (quantity < 0)
        {
            throw new ArgumentException("Quantity cannot be negative.", nameof(quantity));
        }
        Quantity = quantity;
    }

    private void SetCurrentQuantity(decimal? currentQuantity)
    {
        if (currentQuantity.HasValue && currentQuantity.Value < 0)
        {
            throw new ArgumentException("CurrentQuantity cannot be negative.", nameof(currentQuantity));
        }
        CurrentQuantity = currentQuantity;
    }

    private void SetStatus(PolicyContractStatus status)
    {
        Status = status;
    }

    private void SetEmployeeId(Guid? employeeId)
    {
        EmployeeId = employeeId;
    }

    private void SetCancellationDate(DateTime? cancellationDate)
    {
        CancellationDate = cancellationDate;
    }

    private void SetTerminationDate(DateTime? terminationDate)
    {
        TerminationDate = terminationDate;
    }

    private void SetCancellationReasonId(Guid? cancellationReasonId)
    {
        CancellationReasonId = cancellationReasonId;
    }

    private void SetTerminationReasonId(Guid? terminationReasonId)
    {
        TerminationReasonId = terminationReasonId;
    }

    private void SetCancellationDescription(string? cancellationDescription)
    {
        if (cancellationDescription != null && cancellationDescription.Length > 500)
        {
            throw new ArgumentException("CancellationDescription cannot exceed 500 characters.", nameof(cancellationDescription));
        }
        CancellationDescription = cancellationDescription;
    }

    private void SetTerminationDescription(string? terminationDescription)
    {
        if (terminationDescription != null && terminationDescription.Length > 500)
        {
            throw new ArgumentException("TerminationDescription cannot exceed 500 characters.", nameof(terminationDescription));
        }
        TerminationDescription = terminationDescription;
    }

    private void SetIsReciveInvoice(string isReciveInvoice)
    {
        if (string.IsNullOrWhiteSpace(isReciveInvoice))
        {
            throw new ArgumentException("IsReciveInvoice cannot be null or empty.", nameof(isReciveInvoice));
        }
        if (isReciveInvoice.Length > 1)
        {
            throw new ArgumentException("IsReciveInvoice must be a single character (Y or N).", nameof(isReciveInvoice));
        }
        if (isReciveInvoice != "Y" && isReciveInvoice != "N")
        {
            throw new ArgumentException("IsReciveInvoice must be 'Y' or 'N'.", nameof(isReciveInvoice));
        }
        IsReciveInvoice = isReciveInvoice;
    }

    private void SetQuotationId(Guid? quotationId)
    {
        QuotationId = quotationId;
    }

    // Public update methods
    public virtual void UpdateCode(string code)
    {
        SetCode(code);
    }

    public virtual void UpdateName(string name)
    {
        SetName(name);
    }

    public virtual void UpdateType(PolicyContractType type)
    {
        SetType(type);
    }

    public virtual void UpdateStatus(PolicyContractStatus status)
    {
        SetStatus(status);
    }

    public virtual void UpdateExpireDate(DateTime? expireDate)
    {
        SetExpireDate(expireDate);
    }

    public virtual void UpdateInsurerId(Guid? insurerId)
    {
        SetInsurerId(insurerId);
    }

    public virtual void UpdateInsurerContractCode(string? insurerContractCode)
    {
        SetInsurerContractCode(insurerContractCode);
    }

    public virtual void UpdateLobId(Guid? lobId)
    {
        SetLobId(lobId);
    }

    public virtual void UpdateCustomerId(Guid customerId)
    {
        SetCustomerId(customerId);
    }

    public virtual void UpdatePayerInfo(
        string? name, 
        string? email, 
        string? phone, 
        Guid? provinceId, 
        Guid? wardId, 
        string? address, 
        string? fullAddress)
    {
        SetPayerName(name);
        SetPayerEmail(email);
        SetPayerPhone(phone);
        SetPayerProvinceId(provinceId);
        SetPayerWardId(wardId);
        SetPayerAddress(address);
        SetPayerFullAddress(fullAddress);
    }

    public virtual void UpdatePayerTin(string? payerTin)
    {
        SetPayerTin(payerTin);
    }

    public virtual void UpdateDescription(string? description)
    {
        SetDescription(description);
    }

    public virtual void UpdateEffectDate(DateTime effectDate)
    {
        SetEffectDate(effectDate);
    }

    public virtual void UpdateQuantity(decimal quantity)
    {
        SetQuantity(quantity);
    }

    public virtual void UpdateCurrentQuantity(decimal? currentQuantity)
    {
        SetCurrentQuantity(currentQuantity);
    }

    public virtual void UpdateEmployeeId(Guid? employeeId)
    {
        SetEmployeeId(employeeId);
    }

    public virtual void UpdateIsReciveInvoice(string isReciveInvoice)
    {
        SetIsReciveInvoice(isReciveInvoice);
    }

    public virtual void UpdateTerminationDate(DateTime? terminationDate)
    {
        SetTerminationDate(terminationDate);
    }

    public virtual void UpdateTerminationReasonId(Guid? terminationReasonId)
    {
        SetTerminationReasonId(terminationReasonId);
    }
}
