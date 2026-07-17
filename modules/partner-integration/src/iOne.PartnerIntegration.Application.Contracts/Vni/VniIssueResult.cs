using System;
using System.Text.Json.Serialization;

namespace iOne.PartnerIntegration.Vni;

public class VniIssueResult
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = "";

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("data")]
    public object? Data { get; set; }

    [JsonPropertyName("Total")]
    public int Total { get; set; }

    public bool IsSuccess => string.Equals(Code, "000", StringComparison.Ordinal);
}
