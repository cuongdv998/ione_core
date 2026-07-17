using System;
using iOne.Claims;
using iOne.WorkTasks;
using System.Text.Json.Serialization;

namespace iOne.Claim.Claims;

/// <summary>
/// DTO cho một dòng trong danh sách yêu cầu bồi thường được giao cho người đăng nhập (theo work_task).
/// </summary>
public class ClaimTaskDto
{
    /// <summary>
    /// Id của work_task.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Id của claim (business_key trong work_task).
    /// </summary>
    public Guid ClaimId { get; set; }

    public string Code { get; set; } = null!;

    public string? FolderNo { get; set; }

    public string? InsurerName { get; set; }

    public string? InsurerCode { get; set; }

    public string? LobName { get; set; }

    public string? ProductName { get; set; }

    public string? NotifierName { get; set; }

    public DateTime? OpenDate { get; set; }

    public DateTime? NotifyDate { get; set; }

    public string? CarPlate { get; set; }

    public DateTime? IncidentDate { get; set; }

    /// <summary>
    /// Y = Có hiện trường, N = Không.
    /// </summary>
    public string? OnLocation { get; set; }

    public string? OpenEmployeeName { get; set; }

    public string? OpenEmployeePhone { get; set; }

    public ProcessClaimType ProcessClaimType { get; set; }

    public ClaimStatus ClaimStatus { get; set; }

    public WorkTaskStatus WorkTaskStatus { get; set; }

    public DateTime? TaskCreationTime { get; set; }

    public decimal EstimateAmount { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public bool HasActiveOnsiteAssessmentTask { get; set; }

    /// <summary>
    /// True when the latest <c>claim_adjust_at_location</c> for this claim is completed (Done).
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public bool HasFinishedOnsiteAssessment { get; set; }

    public bool CanCancel { get; set; }

    public bool IsReporter { get; set; }

    public bool IsAssignee { get; set; }

    public bool CanReassign { get; set; }

    public bool CanUpdateCompletedDetailedAssessment { get; set; }
}
