namespace iOne.ApiKeys;

/// <summary>
/// Constants for API key authentication (X-Api-Key header, claim names).
/// </summary>
public static class ApiKeyConstants
{
    /// <summary>
    /// HTTP header name for API key (primary).
    /// </summary>
    public const string HeaderName = "X-Api-Key";

    /// <summary>
    /// Alternative header name for API key.
    /// </summary>
    public const string HeaderNameAlternate = "Api-Key";

    /// <summary>
    /// Claim type for the API key ID (stored in principal when authenticated via API key).
    /// </summary>
    public const string ApiKeyIdClaimType = "ApiKeyId";

    /// <summary>
    /// Query parameter names that may carry the API key.
    /// </summary>
    public static readonly string[] QueryParameterNames = { "apiKey", "api_key", "X-Api-Key", "Api-Key" };
}
