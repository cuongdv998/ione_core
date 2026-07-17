using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using iOne.ResTaskCategories;
using iOne.WorkInstances;
using iOne.WorkTasks;
using Volo.Abp.Domain.Entities.Auditing;

namespace iOne.WorkTasks;

[Table("work_task")]
public class WorkTask : FullAuditedAggregateRoot<Guid>
{
    public virtual Guid? WorkInstanceId { get; private set; }

    [Required]
    [MaxLength(50)]
    public virtual string BusinessCode { get; private set; } = null!;

    [Required]
    [MaxLength(50)]
    public virtual string BusinessName { get; private set; } = null!;

    [Required]
    public virtual Guid BusinessKey { get; private set; }

    [MaxLength(250)]
    public virtual string? FormKey { get; private set; }

    [Required]
    [MaxLength(50)]
    public virtual string Code { get; private set; } = null!;

    [Required]
    [MaxLength(250)]
    public virtual string Name { get; private set; } = null!;

    [MaxLength(250)]
    public virtual string? Description { get; private set; }

    [Required]
    public virtual Guid ReporterId { get; private set; }

    [MaxLength(50)]
    public virtual string? BusinessAuthorityCode { get; private set; }

    public virtual Guid? AssigneeId { get; private set; }

    public virtual Guid? AssigneeDepartmentId { get; private set; }

    public virtual Guid? AssigneeOrganizationId { get; private set; }

    public virtual Guid? ResBusinessAssigneeId { get; private set; }

    [Required]
    public virtual WorkTaskStatus Status { get; private set; }

    [Required]
    public virtual WorkTaskPriority Priority { get; private set; }

    [Required]
    public virtual Guid TaskCategoryId { get; private set; }

    [Required]
    public virtual DateTime StartDate { get; private set; }

    [Required]
    public virtual DateTime EndDate { get; private set; }

    public virtual DateTime? ActualStartDate { get; private set; }

    public virtual DateTime? ActualEndDate { get; private set; }

    public virtual Guid? ReasonId { get; private set; }

    [MaxLength(250)]
    public virtual string? ReasonDescription { get; private set; }

    [MaxLength(50)]
    public virtual string? EventName { get; private set; }

    public virtual WorkInstance? WorkInstance { get; set; }

    public virtual ResTaskCategory? TaskCategory { get; set; }

    protected WorkTask()
    {
        // For ORM
    }

    public WorkTask(
        Guid id,
        Guid? workInstanceId,
        string businessCode,
        string businessName,
        Guid businessKey,
        string? formKey,
        string code,
        string? eventName,
        string name,
        string? description,
        Guid reporterId,
        string? businessAuthorityCode,
        Guid? assigneeId,
        Guid? assigneeDepartmentId,
        Guid? assigneeOrganizationId,
        Guid? resBusinessAssigneeId,
        WorkTaskStatus status,
        WorkTaskPriority priority,
        Guid taskCategoryId,
        DateTime startDate,
        DateTime endDate,
        DateTime? actualStartDate,
        DateTime? actualEndDate,
        Guid? reasonId,
        string? reasonDescription)
        : base(id)
    {
        SetWorkInstanceId(workInstanceId);
        SetBusinessCode(businessCode);
        SetBusinessName(businessName);
        SetBusinessKey(businessKey);
        SetFormKey(formKey);
        SetCode(code);
        SetEventName(eventName);
        SetName(name);
        SetDescription(description);
        SetReporterId(reporterId);
        SetBusinessAuthorityCode(businessAuthorityCode);
        SetAssigneeId(assigneeId);
        SetAssigneeDepartmentId(assigneeDepartmentId);
        SetAssigneeOrganizationId(assigneeOrganizationId);
        SetResBusinessAssigneeId(resBusinessAssigneeId);
        SetStatus(status);
        SetPriority(priority);
        SetTaskCategoryId(taskCategoryId);
        SetStartDate(startDate);
        SetEndDate(endDate);
        SetActualStartDate(actualStartDate);
        SetActualEndDate(actualEndDate);
        SetReasonId(reasonId);
        SetReasonDescription(reasonDescription);
    }

    private void SetWorkInstanceId(Guid? workInstanceId)
    {
        WorkInstanceId = workInstanceId;
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

    private void SetBusinessName(string businessName)
    {
        if (string.IsNullOrWhiteSpace(businessName))
        {
            throw new ArgumentException("BusinessName cannot be null or empty.", nameof(businessName));
        }

        if (businessName.Length > 50)
        {
            throw new ArgumentException("BusinessName cannot exceed 50 characters.", nameof(businessName));
        }

        BusinessName = businessName;
    }

    private void SetBusinessKey(Guid businessKey)
    {
        BusinessKey = businessKey;
    }

    private void SetFormKey(string? formKey)
    {
        if (formKey != null && formKey.Length > 250)
        {
            throw new ArgumentException("FormKey cannot exceed 250 characters.", nameof(formKey));
        }

        FormKey = formKey;
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

    private void SetEventName(string? eventName)
    {
        if (eventName != null && eventName.Length > 50)
        {
            throw new ArgumentException("EventName cannot exceed 50 characters.", nameof(eventName));
        }

        EventName = eventName;
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
        if (description != null && description.Length > 250)
        {
            throw new ArgumentException("Description cannot exceed 250 characters.", nameof(description));
        }

        Description = description;
    }

    private void SetReporterId(Guid reporterId)
    {
        ReporterId = reporterId;
    }

    private void SetBusinessAuthorityCode(string? businessAuthorityCode)
    {
        if (businessAuthorityCode != null && businessAuthorityCode.Length > 50)
        {
            throw new ArgumentException("BusinessAuthorityCode cannot exceed 50 characters.", nameof(businessAuthorityCode));
        }

        BusinessAuthorityCode = businessAuthorityCode;
    }

    private void SetAssigneeId(Guid? assigneeId)
    {
        AssigneeId = assigneeId;
    }

    private void SetAssigneeDepartmentId(Guid? assigneeDepartmentId)
    {
        AssigneeDepartmentId = assigneeDepartmentId;
    }

    private void SetAssigneeOrganizationId(Guid? assigneeOrganizationId)
    {
        AssigneeOrganizationId = assigneeOrganizationId;
    }

    private void SetResBusinessAssigneeId(Guid? resBusinessAssigneeId)
    {
        ResBusinessAssigneeId = resBusinessAssigneeId;
    }

    private void SetStatus(WorkTaskStatus status)
    {
        Status = status;
    }

    private void SetPriority(WorkTaskPriority priority)
    {
        Priority = priority;
    }

    private void SetTaskCategoryId(Guid taskCategoryId)
    {
        TaskCategoryId = taskCategoryId;
    }

    private void SetStartDate(DateTime startDate)
    {
        StartDate = startDate;
    }

    private void SetEndDate(DateTime endDate)
    {
        EndDate = endDate;
    }

    private void SetActualStartDate(DateTime? actualStartDate)
    {
        ActualStartDate = actualStartDate;
    }

    private void SetActualEndDate(DateTime? actualEndDate)
    {
        ActualEndDate = actualEndDate;
    }

    private void SetReasonId(Guid? reasonId)
    {
        ReasonId = reasonId;
    }

    private void SetReasonDescription(string? reasonDescription)
    {
        if (reasonDescription != null && reasonDescription.Length > 250)
        {
            throw new ArgumentException("ReasonDescription cannot exceed 250 characters.", nameof(reasonDescription));
        }

        ReasonDescription = reasonDescription;
    }

    public virtual void UpdateStatus(WorkTaskStatus status)
    {
        SetStatus(status);
    }

    public virtual void UpdatePriority(WorkTaskPriority priority)
    {
        SetPriority(priority);
    }

    public virtual void UpdateActualDates(DateTime? actualStartDate, DateTime? actualEndDate)
    {
        SetActualStartDate(actualStartDate);
        SetActualEndDate(actualEndDate);
    }

    /// <summary>
    /// Updates the rejection reason (e.g. when a policy creation request is rejected).
    /// </summary>
    public virtual void UpdateRejectionReason(Guid? reasonId, string? reasonDescription)
    {
        SetReasonId(reasonId);
        SetReasonDescription(reasonDescription);
    }

    /// <summary>
    /// Updates assignee (e.g. after user selects approver on resubmit).
    /// </summary>
    public virtual void UpdateAssignee(
        Guid? assigneeId,
        Guid? assigneeDepartmentId = null,
        Guid? assigneeOrganizationId = null)
    {
        SetAssigneeId(assigneeId);
        SetAssigneeDepartmentId(assigneeDepartmentId);
        SetAssigneeOrganizationId(assigneeOrganizationId);
    }

    /// <summary>
    /// Updates task description (max 250 chars).
    /// </summary>
    public virtual void UpdateDescription(string? description)
    {
        SetDescription(description);
    }
}
