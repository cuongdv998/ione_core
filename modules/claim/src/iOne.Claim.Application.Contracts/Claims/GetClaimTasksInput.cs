using System;
using iOne.Claims;
using iOne.WorkTasks;
using Volo.Abp.Application.Dtos;

namespace iOne.Claim.Claims;

public class GetClaimTasksInput : PagedAndSortedResultRequestDto
{
    public Guid? LobId { get; set; }

    public Guid? InsurerId { get; set; }

    public ProcessClaimType? ProcessClaimType { get; set; }

    public Guid? ProcessDeptId { get; set; }

    public string? NotifierPhone { get; set; }

    public Guid? OpenEmployeeId { get; set; }

    public DateTime? OpenDateFrom { get; set; }

    public DateTime? OpenDateTo { get; set; }

    /// <summary>
    /// Trạng thái claim.
    /// </summary>
    public ClaimStatus? Status { get; set; }

    /// <summary>
    /// Alias tương thích ngược cho Status (claim status).
    /// </summary>
    public ClaimStatus? ClaimStatus { get; set; }

    public WorkTaskStatus? WorkTaskStatus { get; set; }

    public string? CarPlate { get; set; }

    public string? Vin { get; set; }

    public string? EngineNumber { get; set; }

    public string? Code { get; set; }
}
