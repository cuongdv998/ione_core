using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;

namespace iOne.PartnerIntegration.Pti;

/// <summary>
/// Extracts insurer-facing identifiers from PTI GET vehicle/lookup-info JSON (paged envelope).
/// </summary>
public static class PtiVehicleLookupInfoParser
{
    public static void TryExtractLookupInfo(
        string? json,
        out string? insurerPolicyNo,
        out string? insurerContractCode,
        List<PtiCertificateInfoSnapshot> certificateSnapshots)
    {
        insurerPolicyNo = null;
        insurerContractCode = null;
        certificateSnapshots.Clear();

        if (string.IsNullOrWhiteSpace(json))
        {
            return;
        }

        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            if (!root.TryGetProperty("content", out var content) || content.ValueKind != JsonValueKind.Array)
            {
                return;
            }

            var first = content.EnumerateArray().FirstOrDefault();
            if (first.ValueKind != JsonValueKind.Object)
            {
                return;
            }

            if (first.TryGetProperty("contractNumber", out var cn) && cn.ValueKind == JsonValueKind.String)
            {
                insurerContractCode = NullIfWhiteSpace(cn.GetString());
            }

            if (first.TryGetProperty("taskId", out var tid))
            {
                insurerPolicyNo = tid.ValueKind switch
                {
                    JsonValueKind.Number when tid.TryGetInt64(out var n) => n.ToString(CultureInfo.InvariantCulture),
                    JsonValueKind.String => NullIfWhiteSpace(tid.GetString()),
                    _ => null
                };
            }

            if (first.TryGetProperty("certificateInfos", out var certs) && certs.ValueKind == JsonValueKind.Array)
            {
                foreach (var cert in certs.EnumerateArray())
                {
                    if (cert.ValueKind != JsonValueKind.Object)
                    {
                        continue;
                    }

                    var code = ReadOptionalTrimmedString(cert, "certificateCode");
                    var url = ReadOptionalTrimmedString(cert, "url");
                    certificateSnapshots.Add(new PtiCertificateInfoSnapshot(code, url));
                }
            }
        }
        catch
        {
            // ignore malformed JSON; caller keeps inquiry success without suggested refs
        }
    }

    private static string? ReadOptionalTrimmedString(JsonElement parent, string name)
    {
        if (!parent.TryGetProperty(name, out var el))
        {
            return null;
        }

        return el.ValueKind switch
        {
            JsonValueKind.String => NullIfWhiteSpace(el.GetString()),
            JsonValueKind.Number when el.TryGetInt64(out var n) => n.ToString(CultureInfo.InvariantCulture),
            _ => null
        };
    }

    private static string? NullIfWhiteSpace(string? s)
        => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}
