using System.Collections.Generic;

namespace iOne.PartnerIntegration;

public class PartnerPolicyInquiryResult
{
    /// <summary>
    /// Raw partner-side response materialised as a flexible dictionary.
    /// For PTI: the entire lookup-info JSON response object.
    /// </summary>
    public Dictionary<string, object?> PartnerResponse { get; set; } = new();

    /// <summary>PTI vehicle/lookup-info: <c>content[0].taskId</c> only (not lookUpCode or contractNumber).</summary>
    public string? SuggestedInsurerPolicyNo { get; set; }

    /// <summary>PTI: <c>content[0].contractNumber</c> when present.</summary>
    public string? SuggestedInsurerContractCode { get; set; }

    /// <summary>PTI: <c>content[0].certificateInfos[]</c> entries with <c>certificateCode</c> / <c>url</c>.</summary>
    public List<PtiCertificateInfoSnapshot> SuggestedCertificateInfos { get; set; } = new();
}
