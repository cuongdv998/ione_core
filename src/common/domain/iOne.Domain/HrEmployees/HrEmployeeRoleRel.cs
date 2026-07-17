using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;

namespace iOne.HrEmployees;

[Table("hr_employee_role_rel")]
public class HrEmployeeRoleRel : AuditedEntity<Guid>
{
    [Required]
    public virtual Guid EmployeeId { get; private set; }

    [Required]
    public virtual Guid RoleId { get; private set; }

    [Required]
    public virtual DateTime EffectDate { get; private set; }

    public virtual DateTime? ExpireDate { get; private set; }

    // Navigation Properties
    public virtual HrEmployee Employee { get; set; } = null!;
    public virtual HrEmployeeRoles.HrEmployeeRole Role { get; set; } = null!;

    protected HrEmployeeRoleRel()
    {
        // For ORM
    }

    public HrEmployeeRoleRel(
        Guid id,
        Guid employeeId,
        Guid roleId,
        DateTime effectDate,
        DateTime? expireDate = null)
        : base(id)
    {
        SetEmployeeId(employeeId);
        SetRoleId(roleId);
        SetEffectDate(effectDate);
        SetExpireDate(expireDate);
    }

    private void SetEmployeeId(Guid employeeId)
    {
        if (employeeId == Guid.Empty)
        {
            throw new ArgumentException("EmployeeId cannot be empty.", nameof(employeeId));
        }

        EmployeeId = employeeId;
    }

    private void SetRoleId(Guid roleId)
    {
        if (roleId == Guid.Empty)
        {
            throw new ArgumentException("RoleId cannot be empty.", nameof(roleId));
        }

        RoleId = roleId;
    }

    private void SetEffectDate(DateTime effectDate)
    {
        EffectDate = effectDate;
    }

    private void SetExpireDate(DateTime? expireDate)
    {
        // Validate: ExpireDate must be after EffectDate if provided
        if (expireDate.HasValue && expireDate.Value < EffectDate)
        {
            throw new ArgumentException("ExpireDate must be after or equal to EffectDate.", nameof(expireDate));
        }

        ExpireDate = expireDate;
    }

    public virtual void UpdateEffectDate(DateTime effectDate)
    {
        SetEffectDate(effectDate);
        // Re-validate ExpireDate
        if (ExpireDate.HasValue && ExpireDate.Value < effectDate)
        {
            throw new ArgumentException("ExpireDate must be after or equal to EffectDate.", nameof(effectDate));
        }
    }

    public virtual void UpdateExpireDate(DateTime? expireDate)
    {
        SetExpireDate(expireDate);
    }

    public virtual void UpdateRoleId(Guid roleId)
    {
        SetRoleId(roleId);
    }
}

