using System;

namespace iOne.Policy.Policies;

public class GetPolicyClaimLookupInput
{
    public string? CertificateNo { get; set; }

    public string? CarPlate { get; set; }

    public string? Vin { get; set; }

    public string? EngineNumber { get; set; }

    /// <summary>
    /// Optional. If provided, only returns active policies covering this date.
    /// </summary>
    public DateTime? IncidentDate { get; set; }

    /// <summary>
    /// Limits how many policies are processed (before expanding by products).
    /// </summary>
    public int MaxResultCount { get; set; } = 20;
}

