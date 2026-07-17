using System;
using iOne.Claims;
using iOne.WorkTasks;

namespace iOne.Claim.Claims;

public class OnsiteAssessmentTaskDto
{
    public Guid Id { get; set; }

    public Guid ClaimId { get; set; }

    public Guid ClaimAdjustAtLocationId { get; set; }

    public string Code { get; set; } = null!;

    public string? InsurerName { get; set; }

    public string? InsurerCode { get; set; }

    public string? LobName { get; set; }

    public string? NotifierName { get; set; }

    public DateTime? NotifyDate { get; set; }

    public string? CarPlate { get; set; }

    public string? ReporterName { get; set; }

    public Guid ReporterId { get; set; }

    public string? AssigneeName { get; set; }

    public Guid? AssigneeId { get; set; }

    public ProcessClaimType ProcessClaimType { get; set; }

    public WorkTaskStatus WorkTaskStatus { get; set; }

    public bool CanView { get; set; }

    public bool CanAccept { get; set; }

    public bool CanProcess { get; set; }

    public bool CanComplete { get; set; }

    public bool CanCancel { get; set; }

    public bool IsReporter { get; set; }

    public bool IsAssignee { get; set; }

    public bool CanReassign { get; set; }
}
