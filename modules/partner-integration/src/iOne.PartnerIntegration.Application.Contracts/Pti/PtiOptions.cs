namespace iOne.PartnerIntegration.Pti;

public class PtiOptions
{
    /// <summary>Base URL for policy issuance APIs, e.g. https://open-gate-uat.ipas.com.vn/bep-pti-api</summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>Base URL for authentication APIs, e.g. https://open-gate-uat.ipas.com.vn/userid</summary>
    public string AuthBaseUrl { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    /// <summary>partnerName credential supplied by PTI, e.g. PTIPARTNER_IBDS</summary>
    public string PartnerName { get; set; } = string.Empty;

    /// <summary>channelCode value for PTI requests, default KENH004</summary>
    public string ChannelCode { get; set; } = "KENH004";

    /// <summary>File-system path to our RSA private key used for signing/decryption</summary>
    public string PrivateKeyPath { get; set; } = string.Empty;

    /// <summary>EncryptBody or Header — controls how the digital signature is applied</summary>
    public string SignatureMode { get; set; } = "EncryptBody";

    /// <summary>
    /// When EncryptBody mode is used, the passphrase (UTF-8) for our embedded OpenPGP private key
    /// (<c>PtiIoneIbdsPrivateKey</c>). This key is used for:
    /// 1. Signing outgoing request payloads (sign + encrypt)
    /// 2. Decrypting PTI HTTP responses
    /// Omit or leave empty when the key has no passphrase.
    /// </summary>
    public string? PgpEncryptBodyResponsePassphrase { get; set; }

    /// <summary>
    /// When true, the request payload is encrypted/signed before sending (using SignatureMode).
    /// When false, the plain JSON body is sent without any encryption — useful for local dev or
    /// when PTI has not yet enabled signing on their gateway.
    /// Default: true.
    /// </summary>
    public bool EnableSigning { get; set; } = true;
}
