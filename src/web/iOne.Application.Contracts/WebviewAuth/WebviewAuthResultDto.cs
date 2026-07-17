using System.Text.Json.Serialization;

namespace iOne.WebviewAuth;

public class WebviewAuthResultDto
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = null!;

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = "Bearer";

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonPropertyName("data")]
    public WebviewAuthCustomerData? Data { get; set; }
}

public class WebviewAuthCustomerData
{
    [JsonPropertyName("customerCode")]
    public string? CustomerCode { get; set; }

    [JsonPropertyName("customerName")]
    public string? CustomerName { get; set; }

    [JsonPropertyName("customerPhone")]
    public string? CustomerPhone { get; set; }

    [JsonPropertyName("customerEmail")]
    public string? CustomerEmail { get; set; }

    [JsonPropertyName("customerAddress")]
    public string? CustomerAddress { get; set; }
}
