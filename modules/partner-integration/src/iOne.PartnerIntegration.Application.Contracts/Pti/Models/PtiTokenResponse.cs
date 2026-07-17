using System.Text.Json.Serialization;

namespace iOne.PartnerIntegration.Pti.Models;

public class PtiTokenResponse
{
    [JsonPropertyName("statusCode")]
    public int StatusCode { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("token")]
    public string? Token { get; set; }

    public bool IsSuccess => StatusCode == 1 && !string.IsNullOrWhiteSpace(Token);
}
