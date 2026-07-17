using System;

namespace iOne.Policy.PolicyContracts;

public class CreatePolicyContractResponseDto
{
    public bool Success { get; set; }
    public CreatePolicyContractResultDataDto? Data { get; set; }
    public string Message { get; set; } = string.Empty;
}
