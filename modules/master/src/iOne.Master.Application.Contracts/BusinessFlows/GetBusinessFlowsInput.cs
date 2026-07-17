using System;
using iOne.BusinessFlows;
using Volo.Abp.Application.Dtos;

namespace iOne.Master.BusinessFlows;

public class GetBusinessFlowsInput : PagedAndSortedResultRequestDto
{
    public Guid? OrganizationId { get; set; }

    public Guid? InsurerId { get; set; }

    public string? BusinessCode { get; set; }

    public string? WorkflowName { get; set; }

    public string? WorkflowVersion { get; set; }

    public BusinessFlowStatus? Status { get; set; }

    public DateTime? EffectDateFrom { get; set; }

    public DateTime? EffectDateTo { get; set; }

    public DateTime? ExpireDateFrom { get; set; }

    public DateTime? ExpireDateTo { get; set; }
}
