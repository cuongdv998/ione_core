namespace iOne.PartnerIntegration;

/// <summary>
/// One entry from PTI vehicle/lookup-info <c>certificateInfos[]</c> for syncing <see cref="iOne.Policies.PolicyCertificate"/>.
/// </summary>
public sealed record PtiCertificateInfoSnapshot(string? CertificateCode, string? Url);
