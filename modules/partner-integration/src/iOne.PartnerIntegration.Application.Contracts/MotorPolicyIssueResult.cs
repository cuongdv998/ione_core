using System.Collections.Generic;

namespace iOne.PartnerIntegration;

public class MotorPolicyIssueResult
{
    /// <summary>
    /// The outbound correlation id sent to the partner on the create request (for PTI: request.transId).
    /// This is the value persisted to Policy.InsurerPolicyNo and used for payment callback matching and partner lookup.
    /// </summary>
    public string? TransId { get; set; }

    /// <summary>PTI taskId — the insurer-side amendment key returned in the create response. Not stored to InsurerPolicyNo.</summary>
    public string TaskId { get; set; } = string.Empty;

    /// <summary>PTI contractNumber (stored to PolicyContract.InsurerContractCode).</summary>
    public string? ContractNumber { get; set; }

    /// <summary>
    /// Partner-specific payment/response fields returned to the API caller.
    /// For PTI: qrCode, paymentExpired, paymentAmount.
    /// </summary>
    public Dictionary<string, object?> PartnerResponse { get; set; } = new();
}
