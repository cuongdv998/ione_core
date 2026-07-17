using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using iOne.BusinessFlows;
using iOne.WorkInstances;
using Volo.Abp.Domain.Entities.Auditing;

namespace iOne.WorkInstances;

[Table("work_instance")]
public class WorkInstance : CreationAuditedAggregateRoot<Guid>
{
    [Required]
    [MaxLength(50)]
    public virtual string WorkflowInstanceId { get; private set; } = null!;

    [Required]
    public virtual Guid BusinessFlowId { get; private set; }

    [MaxLength(36)]
    public virtual string? BusinessCode { get; private set; }

    [MaxLength(50)]
    public virtual string? BusinessName { get; private set; }

    public virtual Guid? BusinessKey { get; private set; }

    public virtual DateTime? StartDate { get; private set; }

    public virtual DateTime? EndDate { get; private set; }

    [Required]
    public virtual WorkInstanceStatus Status { get; private set; }

    public virtual BusinessFlow? BusinessFlow { get; set; }

    protected WorkInstance()
    {
        // For ORM
    }

    public WorkInstance(
        Guid id,
        string workflowInstanceId,
        Guid businessFlowId,
        string? businessCode,
        string? businessName,
        Guid? businessKey,
        DateTime? startDate,
        DateTime? endDate,
        WorkInstanceStatus status)
        : base(id)
    {
        SetWorkflowInstanceId(workflowInstanceId);
        SetBusinessFlowId(businessFlowId);
        SetBusinessCode(businessCode);
        SetBusinessName(businessName);
        SetBusinessKey(businessKey);
        SetStartDate(startDate);
        SetEndDate(endDate);
        SetStatus(status);
    }

    private void SetWorkflowInstanceId(string workflowInstanceId)
    {
        if (string.IsNullOrWhiteSpace(workflowInstanceId))
        {
            throw new ArgumentException("WorkflowInstanceId cannot be null or empty.", nameof(workflowInstanceId));
        }

        if (workflowInstanceId.Length > 50)
        {
            throw new ArgumentException("WorkflowInstanceId cannot exceed 50 characters.", nameof(workflowInstanceId));
        }

        WorkflowInstanceId = workflowInstanceId;
    }

    private void SetBusinessFlowId(Guid businessFlowId)
    {
        BusinessFlowId = businessFlowId;
    }

    private void SetBusinessCode(string? businessCode)
    {
        if (businessCode != null && businessCode.Length > 36)
        {
            throw new ArgumentException("BusinessCode cannot exceed 36 characters.", nameof(businessCode));
        }

        BusinessCode = businessCode;
    }

    private void SetBusinessName(string? businessName)
    {
        if (businessName != null && businessName.Length > 50)
        {
            throw new ArgumentException("BusinessName cannot exceed 50 characters.", nameof(businessName));
        }

        BusinessName = businessName;
    }

    private void SetBusinessKey(Guid? businessKey)
    {
        BusinessKey = businessKey;
    }

    private void SetStartDate(DateTime? startDate)
    {
        StartDate = startDate;
    }

    private void SetEndDate(DateTime? endDate)
    {
        EndDate = endDate;
    }

    private void SetStatus(WorkInstanceStatus status)
    {
        Status = status;
    }

    public virtual void UpdateStatus(WorkInstanceStatus status)
    {
        SetStatus(status);
    }

    public virtual void UpdateDates(DateTime? startDate, DateTime? endDate)
    {
        SetStartDate(startDate);
        SetEndDate(endDate);
    }
}
