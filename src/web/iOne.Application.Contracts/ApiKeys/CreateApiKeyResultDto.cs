using System;

namespace iOne.ApiKeys;

/// <summary>
/// Result of creating an API key. The full key is returned only once and cannot be retrieved again.
/// </summary>
public class CreateApiKeyResultDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    /// <summary>
    /// The full API key (prefix_secret). Copy and store securely; it will not be shown again.
    /// </summary>
    public string FullKey { get; set; } = null!;
}
