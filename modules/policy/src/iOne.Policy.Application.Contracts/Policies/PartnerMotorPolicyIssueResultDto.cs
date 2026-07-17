using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace iOne.Policy.Policies;

public class PartnerMotorPolicyIssueResultDto
{
    /// <summary>
    /// Partner-specific response payload (e.g. qrCode, paymentExpired, paymentAmount for PTI motorbike).
    /// Null when the partner returned no extra data.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object?>? PartnerResponse { get; set; }
}
