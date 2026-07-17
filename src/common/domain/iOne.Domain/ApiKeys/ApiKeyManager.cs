using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;

namespace iOne.ApiKeys;

public class ApiKeyManager : DomainService
{
    protected IApiKeyRepository ApiKeyRepository { get; }

    public ApiKeyManager(IApiKeyRepository apiKeyRepository)
    {
        ApiKeyRepository = apiKeyRepository;
    }

    /// <summary>
    /// Creates a new API key. The full key (prefix_secret) is returned only once; only the hash is stored.
    /// </summary>
    public virtual async Task<(ApiKey Entity, string FullKey)> CreateAsync(
        Guid userId,
        string name,
        DateTime? expiresAt = null)
    {
        var prefix = await GenerateUniquePrefixAsync();
        var secret = GenerateSecureRandomString(32);
        var keyHash = ComputeSha256Hash(secret);
        var tenantId = CurrentTenant.Id;

        var apiKey = new ApiKey(
            GuidGenerator.Create(),
            userId,
            name,
            prefix,
            keyHash,
            expiresAt,
            tenantId);

        await ApiKeyRepository.InsertAsync(apiKey);
        var fullKey = $"{prefix}_{secret}";
        return (apiKey, fullKey);
    }

    public virtual async Task SetActiveAsync(ApiKey apiKey, bool isActive)
    {
        apiKey.SetActive(isActive);
        await ApiKeyRepository.UpdateAsync(apiKey);
    }

    public virtual async Task RevokeAsync(ApiKey apiKey)
    {
        apiKey.SetActive(false);
        await ApiKeyRepository.UpdateAsync(apiKey);
    }

    /// <summary>
    /// Verifies that the given secret matches the stored hash. Use from authentication handler.
    /// </summary>
    public static bool VerifyHash(string secret, string keyHash)
    {
        if (string.IsNullOrEmpty(secret) || string.IsNullOrEmpty(keyHash))
            return false;
        var computed = ComputeSha256Hash(secret);
        return string.Equals(computed, keyHash, StringComparison.Ordinal);
    }

    /// <summary>
    /// Computes SHA-256 hash of the input (used for storage and verification).
    /// </summary>
    public static string ComputeSha256Hash(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private async Task<string> GenerateUniquePrefixAsync()
    {
        const int prefixLength = 12;
        string prefix;
        int attempts = 0;
        do
        {
            prefix = GenerateSecureRandomString(prefixLength);
            if (await ApiKeyRepository.FindByPrefixAsync(prefix) == null)
                return prefix;
            attempts++;
            if (attempts > 20)
                throw new InvalidOperationException("Could not generate unique API key prefix.");
        } while (true);
    }

    private static string GenerateSecureRandomString(int length)
    {
        const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
        var bytes = new byte[length];
        RandomNumberGenerator.Fill(bytes);
        var result = new char[length];
        for (var i = 0; i < length; i++)
            result[i] = chars[bytes[i] % chars.Length];
        return new string(result);
    }
}
