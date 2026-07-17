namespace iOne.PartnerIntegration.Pti;

/// <summary>
/// Embedded OpenPGP armored private key for decrypting PTI EncryptBody responses
/// (recipient key — same material as docs/pti-integration/private_key_ione.asc).
/// </summary>
public static class PtiIoneIbdsPrivateKey
{
    /// <summary>OpenPGP armored private key block for response-body decryption.</summary>
    public const string IbdsPartnerPgpPrivateKeyArmored =
        """
        -----BEGIN PGP PRIVATE KEY BLOCK-----
        
        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
        3KpaX3KV9Tqm1UtYr16aAJZnvD0ZjpUEeZ+E0Xc=
        =WiG+
        -----END PGP PRIVATE KEY BLOCK-----
        """;
}
