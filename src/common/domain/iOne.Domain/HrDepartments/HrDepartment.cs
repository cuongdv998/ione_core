using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using Volo.Abp.Domain.Entities.Auditing;

namespace iOne.HrDepartments;

[Table("hr_department")]
public class HrDepartment : FullAuditedAggregateRoot<Guid>
{
    // Basic Info
    [Required]
    [MaxLength(25)]
    public virtual string Code { get; private set; } = null!;

    [Required]
    [MaxLength(250)]
    public virtual string Name { get; private set; } = null!;

    [MaxLength(500)]
    public virtual string? Description { get; private set; }

    [Required]
    public virtual HrDepartmentStatus Status { get; private set; }

    [Required]
    public virtual HrDepartmentLevel DeptLevel { get; private set; }

    // Self-Referencing
    public virtual Guid? ParentId { get; private set; }
    public virtual Guid? OrgId { get; private set; }

    // Foreign Keys
    public virtual Guid? TypeId { get; private set; }
    public virtual Guid PartnerId { get; private set; }
    public virtual Guid? ProvinceId { get; private set; }
    public virtual Guid? WardId { get; private set; }
    public virtual Guid? BankId { get; private set; }

    // Address Info
    [MaxLength(500)]
    public virtual string? Address { get; private set; }

    [MaxLength(500)]
    public virtual string? FullAddress { get; private set; }

    // Bank Info
    [MaxLength(50)]
    public virtual string? BankNo { get; private set; }

    // Navigation Properties
    public virtual HrDepartment? Parent { get; set; }
    public virtual HrDepartment Org { get; set; } = null!;
    public virtual HrDepartmentTypes.HrDepartmentType? Type { get; set; }
    public virtual ResPartners.ResPartner Partner { get; set; } = null!;
    public virtual ResProvinces.ResProvince? Province { get; set; }
    public virtual ResWards.ResWard? Ward { get; set; }
    public virtual ResBanks.ResBank? Bank { get; set; }

    // Collection
    public virtual ICollection<HrDepartment> Children { get; set; } = new List<HrDepartment>();

    protected HrDepartment()
    {
        // For ORM
    }

    public HrDepartment(
        Guid id,
        string code,
        string name,
        HrDepartmentStatus status,
        HrDepartmentLevel deptLevel,
        Guid partnerId,
        Guid? orgId = null,
        Guid? parentId = null,
        Guid? typeId = null,
        string? description = null,
        Guid? provinceId = null,
        Guid? wardId = null,
        string? address = null,
        string? fullAddress = null,
        Guid? bankId = null,
        string? bankNo = null)
        : base(id)
    {
        SetCode(code);
        SetName(name);
        SetStatus(status);
        SetDeptLevel(deptLevel);
        SetOrgId(orgId);
        SetPartnerId(partnerId);
        SetParentId(parentId);
        SetTypeId(typeId);
        SetDescription(description);
        SetProvinceId(provinceId);
        SetWardId(wardId);
        SetAddress(address);
        SetFullAddress(fullAddress);
        SetBankId(bankId);
        SetBankNo(bankNo);
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

    private void SetDescription(string? description)
    {
        if (!string.IsNullOrWhiteSpace(description) && description.Length > 500)
        {
            throw new ArgumentException("Description cannot exceed 500 characters.", nameof(description));
        }

        Description = description;
    }

    private void SetStatus(HrDepartmentStatus status)
    {
        Status = status;
    }

    private void SetDeptLevel(HrDepartmentLevel deptLevel)
    {
        DeptLevel = deptLevel;
    }

    private void SetParentId(Guid? parentId)
    {
        ParentId = parentId;
    }

    private void SetOrgId(Guid? orgId)
    {
        OrgId = orgId;
    }

    private void SetTypeId(Guid? typeId)
    {
        TypeId = typeId;
    }

    private void SetPartnerId(Guid partnerId)
    {
        if (partnerId == Guid.Empty)
        {
            throw new ArgumentException("PartnerId cannot be empty.", nameof(partnerId));
        }

        PartnerId = partnerId;
    }

    private void SetProvinceId(Guid? provinceId)
    {
        ProvinceId = provinceId;
    }

    private void SetWardId(Guid? wardId)
    {
        WardId = wardId;
    }

    private void SetAddress(string? address)
    {
        if (!string.IsNullOrWhiteSpace(address) && address.Length > 500)
        {
            throw new ArgumentException("Address cannot exceed 500 characters.", nameof(address));
        }

        Address = address;
    }

    private void SetFullAddress(string? fullAddress)
    {
        if (!string.IsNullOrWhiteSpace(fullAddress) && fullAddress.Length > 500)
        {
            throw new ArgumentException("FullAddress cannot exceed 500 characters.", nameof(fullAddress));
        }

        FullAddress = fullAddress;
    }

    private void SetBankId(Guid? bankId)
    {
        BankId = bankId;
    }

    private void SetBankNo(string? bankNo)
    {
        if (!string.IsNullOrWhiteSpace(bankNo) && bankNo.Length > 50)
        {
            throw new ArgumentException("BankNo cannot exceed 50 characters.", nameof(bankNo));
        }

        BankNo = bankNo;
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

    public virtual void UpdateStatus(HrDepartmentStatus status)
    {
        SetStatus(status);
    }

    public virtual void UpdateDeptLevel(HrDepartmentLevel deptLevel)
    {
        SetDeptLevel(deptLevel);
    }

    public virtual void UpdateParentId(Guid? parentId)
    {
        SetParentId(parentId);
    }

    public virtual void UpdateOrgId(Guid? orgId)
    {
        SetOrgId(orgId);
    }

    public virtual void UpdateTypeId(Guid? typeId)
    {
        SetTypeId(typeId);
    }

    public virtual void UpdateProvinceId(Guid? provinceId)
    {
        SetProvinceId(provinceId);
    }

    public virtual void UpdateWardId(Guid? wardId)
    {
        SetWardId(wardId);
    }

    public virtual void UpdateAddress(string? address)
    {
        SetAddress(address);
    }

    public virtual void UpdateFullAddress(string? fullAddress)
    {
        SetFullAddress(fullAddress);
    }

    public virtual void UpdateBankId(Guid? bankId)
    {
        SetBankId(bankId);
    }

    public virtual void UpdateBankNo(string? bankNo)
    {
        SetBankNo(bankNo);
    }
}

