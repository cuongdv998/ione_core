using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using Volo.Abp.Domain.Entities.Auditing;

namespace iOne.HrEmployees;

[Table("hr_employee")]
public class HrEmployee : FullAuditedAggregateRoot<Guid>
{
    // Basic Info
    [Required]
    [MaxLength(50)]
    public virtual string Code { get; private set; } = null!;

    [Required]
    [MaxLength(50)]
    public virtual string FullName { get; private set; } = null!;

    [Required]
    public virtual HrEmployeeStatus Status { get; private set; }

    // Foreign Keys
    public virtual Guid? PositionId { get; private set; }
    public virtual Guid? LevelId { get; private set; }
    public virtual Guid? PartnerId { get; private set; }
    public virtual Guid? OrgId { get; private set; }
    public virtual Guid DepartmentId { get; private set; }
    public virtual bool? IsManager { get; private set; }
    public virtual Guid? ManagerId { get; private set; }
    public virtual Guid? ProvinceId { get; private set; }
    public virtual Guid? WardId { get; private set; }
    public virtual Guid? UserId { get; private set; }

    // Address Info
    [MaxLength(250)]
    public virtual string? Address { get; private set; }

    [MaxLength(500)]
    public virtual string? FullAddress { get; private set; }

    // Contact Info
    [MaxLength(15)]
    public virtual string? Phone { get; private set; }

    [MaxLength(50)]
    public virtual string? Email { get; private set; }

    // Navigation Properties
    public virtual HrEmployeePositions.HrEmployeePosition? Position { get; set; }
    public virtual HrEmployeeLevels.HrEmployeeLevel? Level { get; set; }
    public virtual ResPartners.ResPartner? Partner { get; set; }
    public virtual HrDepartments.HrDepartment? Org { get; set; }
    public virtual HrDepartments.HrDepartment Department { get; set; } = null!;
    public virtual HrEmployee? Manager { get; set; }
    public virtual ResProvinces.ResProvince? Province { get; set; }
    public virtual ResWards.ResWard? Ward { get; set; }

    // Collections
    public virtual ICollection<HrEmployeeRoleRel> Roles { get; set; } = new List<HrEmployeeRoleRel>();
    public virtual ICollection<HrEmployee> ManagedEmployees { get; set; } = new List<HrEmployee>();
    public virtual ICollection<Policies.Policy> PoliciesAsSeller { get; set; } = new List<Policies.Policy>();
    public virtual ICollection<Policies.Policy> PoliciesAsImplementer { get; set; } = new List<Policies.Policy>();

    protected HrEmployee()
    {
        // For ORM
    }

    public HrEmployee(
        Guid id,
        string code,
        string fullName,
        HrEmployeeStatus status,
        Guid departmentId,
        Guid? positionId = null,
        Guid? levelId = null,
        Guid? partnerId = null,
        Guid? orgId = null,
        bool? isManager = null,
        Guid? managerId = null,
        Guid? provinceId = null,
        Guid? wardId = null,
        string? address = null,
        string? fullAddress = null,
        string? phone = null,
        string? email = null,
        Guid? userId = null)
        : base(id)
    {
        SetCode(code);
        SetFullName(fullName);
        SetStatus(status);
        SetDepartmentId(departmentId);
        SetPositionId(positionId);
        SetLevelId(levelId);
        SetPartnerId(partnerId);
        SetOrgId(orgId);
        SetIsManager(isManager);
        SetManagerId(managerId);
        SetProvinceId(provinceId);
        SetWardId(wardId);
        SetAddress(address);
        SetFullAddress(fullAddress);
        SetPhone(phone);
        SetEmail(email);
        SetUserId(userId);
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

    private void SetFullName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new ArgumentException("FullName cannot be null or empty.", nameof(fullName));
        }

        if (fullName.Length > 50)
        {
            throw new ArgumentException("FullName cannot exceed 50 characters.", nameof(fullName));
        }

        FullName = fullName;
    }

    private void SetStatus(HrEmployeeStatus status)
    {
        Status = status;
    }

    private void SetDepartmentId(Guid departmentId)
    {
        if (departmentId == Guid.Empty)
        {
            throw new ArgumentException("DepartmentId cannot be empty.", nameof(departmentId));
        }

        DepartmentId = departmentId;
    }

    private void SetPositionId(Guid? positionId)
    {
        PositionId = positionId;
    }

    private void SetLevelId(Guid? levelId)
    {
        LevelId = levelId;
    }

    private void SetPartnerId(Guid? partnerId)
    {
        PartnerId = partnerId;
    }

    private void SetOrgId(Guid? orgId)
    {
        OrgId = orgId;
    }

    private void SetIsManager(bool? isManager)
    {
        IsManager = isManager;
    }

    private void SetManagerId(Guid? managerId)
    {
        ManagerId = managerId;
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
        if (!string.IsNullOrWhiteSpace(address) && address.Length > 250)
        {
            throw new ArgumentException("Address cannot exceed 250 characters.", nameof(address));
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

    private void SetPhone(string? phone)
    {
        if (!string.IsNullOrWhiteSpace(phone) && phone.Length > 15)
        {
            throw new ArgumentException("Phone cannot exceed 15 characters.", nameof(phone));
        }

        Phone = phone;
    }

    private void SetEmail(string? email)
    {
        if (!string.IsNullOrWhiteSpace(email) && email.Length > 50)
        {
            throw new ArgumentException("Email cannot exceed 50 characters.", nameof(email));
        }

        Email = email;
    }

    private void SetUserId(Guid? userId)
    {
        UserId = userId;
    }

    // Public update methods (KHÔNG có UpdateCode - Code is immutable)
    public virtual void UpdateFullName(string fullName)
    {
        SetFullName(fullName);
    }

    public virtual void UpdateStatus(HrEmployeeStatus status)
    {
        SetStatus(status);
    }

    public virtual void UpdatePositionId(Guid? positionId)
    {
        SetPositionId(positionId);
    }

    public virtual void UpdateLevelId(Guid? levelId)
    {
        SetLevelId(levelId);
    }

    public virtual void UpdatePartnerId(Guid? partnerId)
    {
        SetPartnerId(partnerId);
    }

    public virtual void UpdateOrgId(Guid? orgId)
    {
        SetOrgId(orgId);
    }

    public virtual void UpdateDepartmentId(Guid departmentId)
    {
        SetDepartmentId(departmentId);
    }

    public virtual void UpdateIsManager(bool? isManager)
    {
        SetIsManager(isManager);
    }

    public virtual void UpdateManagerId(Guid? managerId)
    {
        SetManagerId(managerId);
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

    public virtual void UpdatePhone(string? phone)
    {
        SetPhone(phone);
    }

    public virtual void UpdateEmail(string? email)
    {
        SetEmail(email);
    }

    public virtual void UpdateUserId(Guid? userId)
    {
        SetUserId(userId);
    }
}

