using System;

namespace iOne.Policy.PolicyContracts;

/// <summary>
/// Minimal data returned after creating a policy contract to avoid serialization issues with full DTO.
/// </summary>
public class CreatePolicyContractResultDataDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
}
