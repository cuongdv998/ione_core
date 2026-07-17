using Volo.Abp;

namespace iOne.PartnerIntegration.Pti;

/// <summary>
/// Raised when PTI returns a non-success HTTP status. Carries the raw response body and status
/// so callers can persist audit logs even when the ambient transaction rolls back.
/// </summary>
public sealed class PtiPartnerHttpException : BusinessException
{
    public int HttpStatusCode { get; }

    /// <summary>
    /// Response body persisted for auditing: decrypted plaintext when EncryptBody decryption ran;
    /// otherwise wire body as returned by PTI.
    /// </summary>
    public string RawResponseBody { get; }

    /// <summary>Request URL when built on the client (e.g. GET lookup), if available.</summary>
    public string? RequestUrl { get; }

    public PtiPartnerHttpException(
        string localizationCode,
        int httpStatusCode,
        string? rawResponseBody,
        string? friendlyMessage,
        string? requestUrl = null)
        : base(
            localizationCode,
            message: friendlyMessage ?? rawResponseBody ?? $"HTTP {httpStatusCode}")
    {
        HttpStatusCode = httpStatusCode;
        RawResponseBody = rawResponseBody ?? string.Empty;
        RequestUrl = requestUrl;

        WithData("StatusCode", httpStatusCode);
        WithData("Error", friendlyMessage ?? rawResponseBody ?? string.Empty);
    }
}
