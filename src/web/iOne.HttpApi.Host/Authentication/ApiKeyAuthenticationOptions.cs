using Microsoft.AspNetCore.Authentication;

namespace iOne.Authentication;

public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
    public const string DefaultScheme = "ApiKey";

    /// <summary>
    /// HTTP header name to resolve the API key from. Default is X-Api-Key.
    /// </summary>
    public string HeaderName { get; set; } = "X-Api-Key";
}
