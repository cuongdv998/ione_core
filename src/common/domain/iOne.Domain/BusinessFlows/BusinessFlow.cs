using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;

namespace iOne.BusinessFlows;

[Table("business_flow")]
public class BusinessFlow : FullAuditedAggregateRoot<Guid>
{
    public virtual Guid? OrganizationId { get; private set; }

    public virtual Guid? InsurerId { get; private set; }

    [Required]
    [MaxLength(50)]
    public virtual string BusinessCode { get; private set; } = null!;

    [Required]
    [MaxLength(50)]
    public virtual string WorkflowName { get; private set; } = null!;

    [Required]
    [MaxLength(50)]
    public virtual string WorkflowVersion { get; private set; } = null!;

    [Required]
    public virtual DateTime EffectDate { get; private set; }

    public virtual DateTime? ExpireDate { get; private set; }

    [Required]
    public virtual BusinessFlowStatus Status { get; private set; }

    protected BusinessFlow()
    {
        // For ORM
    }

    public BusinessFlow(
        Guid id,
        Guid? organizationId,
        Guid? insurerId,
        string businessCode,
        string workflowName,
        string workflowVersion,
        DateTime effectDate,
        DateTime? expireDate,
        BusinessFlowStatus status)
        : base(id)
    {
        SetOrganizationId(organizationId);
        SetInsurerId(insurerId);
        SetBusinessCode(businessCode);
        SetWorkflowName(workflowName);
        SetWorkflowVersion(workflowVersion);
        SetEffectDate(effectDate);
        SetExpireDate(expireDate);
        SetStatus(status);
    }

    private void SetOrganizationId(Guid? organizationId)
    {
        OrganizationId = organizationId;
    }

    private void SetInsurerId(Guid? insurerId)
    {
        InsurerId = insurerId;
    }

    private void SetBusinessCode(string businessCode)
    {
        if (string.IsNullOrWhiteSpace(businessCode))
        {
            throw new ArgumentException("BusinessCode cannot be null or empty.", nameof(businessCode));
        }

        if (businessCode.Length > 50)
        {
            throw new ArgumentException("BusinessCode cannot exceed 50 characters.", nameof(businessCode));
        }

        BusinessCode = businessCode;
    }

    private void SetWorkflowName(string workflowName)
    {
        if (string.IsNullOrWhiteSpace(workflowName))
        {
            throw new ArgumentException("WorkflowName cannot be null or empty.", nameof(workflowName));
        }

        if (workflowName.Length > 50)
        {
            throw new ArgumentException("WorkflowName cannot exceed 50 characters.", nameof(workflowName));
        }

        WorkflowName = workflowName;
    }

    private void SetWorkflowVersion(string workflowVersion)
    {
        if (string.IsNullOrWhiteSpace(workflowVersion))
        {
            throw new ArgumentException("WorkflowVersion cannot be null or empty.", nameof(workflowVersion));
        }

        if (workflowVersion.Length > 50)
        {
            throw new ArgumentException("WorkflowVersion cannot exceed 50 characters.", nameof(workflowVersion));
        }

        WorkflowVersion = workflowVersion;
    }

    private void SetEffectDate(DateTime effectDate)
    {
        EffectDate = effectDate;
    }

    private void SetExpireDate(DateTime? expireDate)
    {
        ExpireDate = expireDate;
    }

    private void SetStatus(BusinessFlowStatus status)
    {
        Status = status;
    }

    public virtual void UpdateWorkflowInfo(string workflowName, string workflowVersion, DateTime? expireDate)
    {
        SetWorkflowName(workflowName);
        SetWorkflowVersion(workflowVersion);
        SetExpireDate(expireDate);
    }

    public virtual void UpdateStatus(BusinessFlowStatus status)
    {
        SetStatus(status);
    }
}
