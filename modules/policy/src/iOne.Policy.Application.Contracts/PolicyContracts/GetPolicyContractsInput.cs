using System;
using iOne.PolicyContracts;
using Volo.Abp.Application.Dtos;

namespace iOne.Policy.PolicyContracts;

public class GetPolicyContractsInput : PagedAndSortedResultRequestDto
{
    public string? Code { get; set; }

    public string? Name { get; set; }

    public Guid? InsurerId { get; set; }

    public Guid? CustomerId { get; set; }

    public Guid? LobId { get; set; }

    public Guid? EmployeeId { get; set; }

    public PolicyContractType? Type { get; set; }

    public PolicyContractStatus? Status { get; set; }

    public PolicyContractStatus[]? Statuses { get; set; }
}
