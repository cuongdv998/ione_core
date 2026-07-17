using System;

namespace iOne.Policy.Policies;

/// <summary>
/// DTO for a work task row in the policy record history (work_task where business_name='policy', business_key=policyId).
/// </summary>
public class PolicyWorkTaskHistoryItemDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Assignee display name (from HrEmployee). Shown in record history "Tên" column instead of business authority name.
    /// </summary>
    public string? AssigneeName { get; set; }

    public string Code { get; set; } = string.Empty;

    public string? EventName { get; set; }

    /// <summary>
    /// WorkTaskStatus enum value (e.g. New=0, InProgress=1, Completed=2, etc.).
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// WorkTaskStatus enum name for display (e.g. "New", "Approved", "WaitApprove").
    /// </summary>
    public string StatusText { get; set; } = string.Empty;

    public DateTime CreationTime { get; set; }

    public string? Description { get; set; }

    public string? BusinessCode { get; set; }

    /// <summary>
    /// Localized display name for BusinessCode, from admin_config where code = 'BUSINESS_CODE' and sub_code = BusinessCode.
    /// </summary>
    public string? BusinessCodeDisplayName { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }
}
