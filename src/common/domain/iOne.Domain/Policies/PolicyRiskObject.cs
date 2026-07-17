using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using iOne.ResObjectTypes;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;

namespace iOne.Policies;

[Table("policy_risk_object")]
public class PolicyRiskObject : FullAuditedAggregateRoot<Guid>, IEntity<Guid>
{
    protected PolicyRiskObject()
    {
        // For ORM
    }

    public PolicyRiskObject(
        Guid id,
        Guid policyId,
        Guid policyVersionId,
        Guid objectTypeId,
        string? repName = null,
        string? repIdNo = null,
        string? repPassport = null,
        string? repPhone = null,
        string? repEmail = null,
        Guid? repProvinceId = null,
        Guid? repWardId = null,
        string? repAddress = null,
        string? repFullAddress = null,
        Guid? riskObjectProvinceId = null,
        Guid? riskObjectWardId = null,
        string? riskObjectAddress = null,
        string? riskObjectFullAddress = null,
        double? riskObjectLat = null,
        double? riskObjectLong = null)
        : base(id)
    {
        SetPolicyId(policyId);
        SetPolicyVersionId(policyVersionId);
        SetObjectTypeId(objectTypeId);
        SetRepName(repName);
        SetRepIdNo(repIdNo);
        SetRepPassport(repPassport);
        SetRepPhone(repPhone);
        SetRepEmail(repEmail);
        SetRepProvinceId(repProvinceId);
        SetRepWardId(repWardId);
        SetRepAddress(repAddress);
        SetRepFullAddress(repFullAddress);
        SetRiskObjectProvinceId(riskObjectProvinceId);
        SetRiskObjectWardId(riskObjectWardId);
        SetRiskObjectAddress(riskObjectAddress);
        SetRiskObjectFullAddress(riskObjectFullAddress);
        SetRiskObjectLat(riskObjectLat);
        SetRiskObjectLong(riskObjectLong);
    }

    [Required] public virtual Guid PolicyId { get; private set; }

    [Required] public virtual Guid PolicyVersionId { get; private set; }

    [Required] public virtual Guid ObjectTypeId { get; private set; }

    [MaxLength(250)] public virtual string? RepName { get; private set; }

    [MaxLength(15)] public virtual string? RepIdNo { get; private set; }

    [MaxLength(15)] public virtual string? RepPassport { get; private set; }

    [MaxLength(15)] public virtual string? RepPhone { get; private set; }

    [MaxLength(50)] public virtual string? RepEmail { get; private set; }

    public virtual Guid? RepProvinceId { get; private set; }

    public virtual Guid? RepWardId { get; private set; }

    [MaxLength(250)] public virtual string? RepAddress { get; private set; }

    [MaxLength(500)] public virtual string? RepFullAddress { get; private set; }

    public virtual Guid? RiskObjectProvinceId { get; private set; }

    public virtual Guid? RiskObjectWardId { get; private set; }

    [MaxLength(250)] public virtual string? RiskObjectAddress { get; private set; }

    [MaxLength(500)] public virtual string? RiskObjectFullAddress { get; private set; }

    [Column(TypeName = "FLOAT8")] public virtual double? RiskObjectLat { get; private set; }

    [Column(TypeName = "FLOAT8")] public virtual double? RiskObjectLong { get; private set; }

    // Navigation Properties
    public virtual Policy Policy { get; set; } = null!;
    public virtual PolicyVersion PolicyVersion { get; set; } = null!;
    public virtual ResObjectType ObjectType { get; set; } = null!;
    public virtual ICollection<PolicyRiskMotor> PolicyRiskMotors { get; set; } = new List<PolicyRiskMotor>();
    public virtual ICollection<PolicyRiskObjectDocument> Documents { get; set; } = new List<PolicyRiskObjectDocument>();

    // Private setters with validation
    private void SetPolicyId(Guid policyId)
    {
        if (policyId == Guid.Empty)
        {
            throw new ArgumentException("PolicyId cannot be empty.", nameof(policyId));
        }

        PolicyId = policyId;
    }

    private void SetPolicyVersionId(Guid policyVersionId)
    {
        if (policyVersionId == Guid.Empty)
        {
            throw new ArgumentException("PolicyVersionId cannot be empty.", nameof(policyVersionId));
        }

        PolicyVersionId = policyVersionId;
    }

    private void SetObjectTypeId(Guid objectTypeId)
    {
        if (objectTypeId == Guid.Empty)
        {
            throw new ArgumentException("ObjectTypeId cannot be empty.", nameof(objectTypeId));
        }

        ObjectTypeId = objectTypeId;
    }

    private void SetRepName(string? repName)
    {
        if (repName != null && repName.Length > 250)
        {
            throw new ArgumentException("RepName cannot exceed 250 characters.", nameof(repName));
        }

        RepName = repName;
    }

    private void SetRepIdNo(string? repIdNo)
    {
        if (repIdNo != null && repIdNo.Length > 15)
        {
            throw new ArgumentException("RepIdNo cannot exceed 15 characters.", nameof(repIdNo));
        }

        RepIdNo = repIdNo;
    }

    private void SetRepPassport(string? repPassport)
    {
        if (repPassport != null && repPassport.Length > 15)
        {
            throw new ArgumentException("RepPassport cannot exceed 15 characters.", nameof(repPassport));
        }

        RepPassport = repPassport;
    }

    private void SetRepPhone(string? repPhone)
    {
        if (repPhone != null && repPhone.Length > 15)
        {
            throw new ArgumentException("RepPhone cannot exceed 15 characters.", nameof(repPhone));
        }

        RepPhone = repPhone;
    }

    private void SetRepEmail(string? repEmail)
    {
        if (repEmail != null && repEmail.Length > 50)
        {
            throw new ArgumentException("RepEmail cannot exceed 50 characters.", nameof(repEmail));
        }

        RepEmail = repEmail;
    }

    private void SetRepProvinceId(Guid? repProvinceId)
    {
        RepProvinceId = repProvinceId;
    }

    private void SetRepWardId(Guid? repWardId)
    {
        RepWardId = repWardId;
    }

    private void SetRepAddress(string? repAddress)
    {
        if (repAddress != null && repAddress.Length > 250)
        {
            throw new ArgumentException("RepAddress cannot exceed 250 characters.", nameof(repAddress));
        }

        RepAddress = repAddress;
    }

    private void SetRepFullAddress(string? repFullAddress)
    {
        if (repFullAddress != null && repFullAddress.Length > 500)
        {
            throw new ArgumentException("RepFullAddress cannot exceed 500 characters.", nameof(repFullAddress));
        }

        RepFullAddress = repFullAddress;
    }

    private void SetRiskObjectProvinceId(Guid? riskObjectProvinceId)
    {
        RiskObjectProvinceId = riskObjectProvinceId;
    }

    private void SetRiskObjectWardId(Guid? riskObjectWardId)
    {
        RiskObjectWardId = riskObjectWardId;
    }

    private void SetRiskObjectAddress(string? riskObjectAddress)
    {
        if (riskObjectAddress != null && riskObjectAddress.Length > 250)
        {
            throw new ArgumentException("RiskObjectAddress cannot exceed 250 characters.", nameof(riskObjectAddress));
        }

        RiskObjectAddress = riskObjectAddress;
    }

    private void SetRiskObjectFullAddress(string? riskObjectFullAddress)
    {
        if (riskObjectFullAddress != null && riskObjectFullAddress.Length > 500)
        {
            throw new ArgumentException("RiskObjectFullAddress cannot exceed 500 characters.",
                nameof(riskObjectFullAddress));
        }

        RiskObjectFullAddress = riskObjectFullAddress;
    }

    private void SetRiskObjectLat(double? riskObjectLat)
    {
        RiskObjectLat = riskObjectLat;
    }

    private void SetRiskObjectLong(double? riskObjectLong)
    {
        RiskObjectLong = riskObjectLong;
    }

    // Public update methods
    public virtual void UpdateRepName(string? repName)
    {
        SetRepName(repName);
    }

    public virtual void UpdateRepIdNo(string? repIdNo)
    {
        SetRepIdNo(repIdNo);
    }

    public virtual void UpdateRepPassport(string? repPassport)
    {
        SetRepPassport(repPassport);
    }

    public virtual void UpdateRepPhone(string? repPhone)
    {
        SetRepPhone(repPhone);
    }

    public virtual void UpdateRepEmail(string? repEmail)
    {
        SetRepEmail(repEmail);
    }

    public virtual void UpdateRepProvinceId(Guid? repProvinceId)
    {
        SetRepProvinceId(repProvinceId);
    }

    public virtual void UpdateRepWardId(Guid? repWardId)
    {
        SetRepWardId(repWardId);
    }

    public virtual void UpdateRepAddress(string? repAddress)
    {
        SetRepAddress(repAddress);
    }

    public virtual void UpdateRepFullAddress(string? repFullAddress)
    {
        SetRepFullAddress(repFullAddress);
    }

    public virtual void UpdateRiskObjectProvinceId(Guid? riskObjectProvinceId)
    {
        SetRiskObjectProvinceId(riskObjectProvinceId);
    }

    public virtual void UpdateRiskObjectWardId(Guid? riskObjectWardId)
    {
        SetRiskObjectWardId(riskObjectWardId);
    }

    public virtual void UpdateRiskObjectAddress(string? riskObjectAddress)
    {
        SetRiskObjectAddress(riskObjectAddress);
    }

    public virtual void UpdateRiskObjectFullAddress(string? riskObjectFullAddress)
    {
        SetRiskObjectFullAddress(riskObjectFullAddress);
    }

    public virtual void UpdateRiskObjectLat(double? riskObjectLat)
    {
        SetRiskObjectLat(riskObjectLat);
    }

    public virtual void UpdateRiskObjectLong(double? riskObjectLong)
    {
        SetRiskObjectLong(riskObjectLong);
    }
}