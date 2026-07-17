using System.Collections.Generic;

namespace iOne.Policy.Policies;

public class PartnerPolicyInquiryResultDto
{
    /// <summary>
    /// Raw partner-side response as a flexible map.
    /// For PTI: the entire lookup-info JSON response object.
    /// </summary>
    public Dictionary<string, object?>? PartnerResponse { get; set; }
}
