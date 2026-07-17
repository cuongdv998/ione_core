using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;

namespace iOne.ResBusinessAssignees;

[Table("res_business_assignee")]
public class ResBusinessAssignee : FullAuditedAggregateRoot<Guid>
{
    public virtual Guid? OrganizationId { get; private set; }

    [Required]
    [MaxLength(50)]
    public virtual string BusinessCode { get; private set; } = null!;

    [Required]
    [MaxLength(50)]
    public virtual string AuthorityCode { get; private set; } = null!;

    [Required]
    public virtual ResBusinessAssigneeType AssigneeType { get; private set; }

    [Required]
    public virtual DateTime EffectDate { get; private set; }

    public virtual DateTime? ExpireDate { get; private set; }

    [MaxLength(50)]
    public virtual string? AssigneeRole { get; private set; }

    public virtual Guid? AssigneeId { get; private set; }

    public virtual Guid? DepartmentId { get; private set; }

    public virtual ResBusinessAssigneeDepartmentLevel? DepartmentLevel { get; private set; }

    [Required]
    public virtual ResBusinessAssigneeStatus Status { get; private set; }

    protected ResBusinessAssignee()
    {
    }

    public ResBusinessAssignee(
        Guid id,
        string businessCode,
        string authorityCode,
        ResBusinessAssigneeType assigneeType,
        DateTime effectDate,
        ResBusinessAssigneeStatus status,
        Guid? organizationId = null,
        DateTime? expireDate = null,
        string? assigneeRole = null,
        Guid? assigneeId = null,
        Guid? departmentId = null,
        ResBusinessAssigneeDepartmentLevel? departmentLevel = null)
        : base(id)
    {
        OrganizationId = organizationId;
        SetBusinessCode(businessCode);
        SetAuthorityCode(authorityCode);
        AssigneeType = assigneeType;
        ExpireDate = expireDate;
        SetEffectDate(effectDate);
        AssigneeRole = assigneeRole;
        AssigneeId = assigneeId;
        DepartmentId = departmentId;
        DepartmentLevel = departmentLevel;
        Status = status;
    }

    private void SetBusinessCode(string businessCode)
    {
        if (string.IsNullOrWhiteSpace(businessCode))
            throw new ArgumentException("BusinessCode cannot be null or empty.", nameof(businessCode));
        if (businessCode.Length > 50)
            throw new ArgumentException("BusinessCode cannot exceed 50 characters.", nameof(businessCode));
        BusinessCode = businessCode;
    }

    private void SetAuthorityCode(string authorityCode)
    {
        if (string.IsNullOrWhiteSpace(authorityCode))
            throw new ArgumentException("AuthorityCode cannot be null or empty.", nameof(authorityCode));
        if (authorityCode.Length > 50)
            throw new ArgumentException("AuthorityCode cannot exceed 50 characters.", nameof(authorityCode));
        AuthorityCode = authorityCode;
    }

    private void SetEffectDate(DateTime effectDate)
    {
        if (ExpireDate.HasValue && ExpireDate.Value < effectDate)
            throw new ArgumentException("ExpireDate must be after or equal to EffectDate.", nameof(effectDate));
        EffectDate = effectDate;
    }

    /// <summary>
    /// Chỉ cho phép sửa nhân viên duyệt khi update.
    /// </summary>
    public virtual void UpdateAssigneeId(Guid? assigneeId)
    {
        AssigneeId = assigneeId;
    }

    /// <summary>
    /// Cho phép sửa ngày hết hạn khi update. ExpireDate phải sau hoặc bằng EffectDate.
    /// </summary>
    public virtual void UpdateExpireDate(DateTime? expireDate)
    {
        if (expireDate.HasValue && expireDate.Value < EffectDate)
            throw new ArgumentException("ExpireDate must be after or equal to EffectDate.", nameof(expireDate));
        ExpireDate = expireDate;
    }

    public virtual void UpdateStatus(ResBusinessAssigneeStatus status)
    {
        Status = status;
    }
}
