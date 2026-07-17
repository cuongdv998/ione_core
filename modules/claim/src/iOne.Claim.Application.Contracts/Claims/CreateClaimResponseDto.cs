using System;
using System.Collections.Generic;

namespace iOne.Claim.Claims;

public class CreateClaimResponseDto
{
    public Guid ClaimId { get; set; }

    public string Code { get; set; } = null!;

    public string Status { get; set; } = null!;

    public string SnapshotLink { get; set; } = null!;

    public List<PolicyDto> Policies { get; set; } = new List<PolicyDto>();
}
