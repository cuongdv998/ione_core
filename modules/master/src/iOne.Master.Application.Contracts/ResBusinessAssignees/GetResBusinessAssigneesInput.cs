using System;
using iOne.ResBusinessAssignees;
using Volo.Abp.Application.Dtos;

namespace iOne.Master.ResBusinessAssignees;

public class GetResBusinessAssigneesInput : PagedAndSortedResultRequestDto
{
    public Guid? OrganizationId { get; set; }

    public string? BusinessCode { get; set; }

    public string? AuthorityCode { get; set; }

    public ResBusinessAssigneeType? AssigneeType { get; set; }

    public Guid? DepartmentId { get; set; }

    public Guid? AssigneeId { get; set; }

    public ResBusinessAssigneeStatus? Status { get; set; }

    public DateTime? EffectDateFrom { get; set; }

    public DateTime? EffectDateTo { get; set; }
}
