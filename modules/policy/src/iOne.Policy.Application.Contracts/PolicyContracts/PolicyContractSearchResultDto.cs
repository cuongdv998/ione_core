using System;

namespace iOne.Policy.PolicyContracts;

public class PolicyContractSearchResultDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public Guid? LobId { get; set; }
    public string Type { get; set; } = null!;
    public Guid? InsurerId { get; set; }
    public Guid CustomerId { get; set; }
    public decimal Quantity { get; set; }
    public decimal? CurrentQuantity { get; set; }
    public string EffectDate { get; set; } = null!; // dd/MM/yyyy
    public string? ExpireDate { get; set; }          // dd/MM/yyyy
    public string Status { get; set; } = null!;
    public string CreationTime { get; set; } = null!; // HH:mm dd/MM/yyyy
    public Guid? CreatorId { get; set; }
    public string? CreatorName { get; set; }
}
