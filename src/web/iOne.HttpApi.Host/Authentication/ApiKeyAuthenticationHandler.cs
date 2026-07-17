using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using iOne.ApiKeys;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volo.Abp.Security.Claims;

namespace iOne.Authentication;

public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
{
    private const string PrefixSecretSeparator = "_";
    private readonly IApiKeyRepository _apiKeyRepository;

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<ApiKeyAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IApiKeyRepository apiKeyRepository)
        : base(options, logger, encoder)
    {
        _apiKeyRepository = apiKeyRepository;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var apiKey = ResolveApiKey();
        if (string.IsNullOrWhiteSpace(apiKey))
            return AuthenticateResult.NoResult();

        var parts = apiKey.Split(PrefixSecretSeparator, 2, StringSplitOptions.None);
        if (parts.Length != 2 || string.IsNullOrEmpty(parts[0]) || string.IsNullOrEmpty(parts[1]))
            return AuthenticateResult.Fail("Invalid API key format. Expected prefix_secret.");

        var prefix = parts[0];
        var secret = parts[1];

        var apiKeyEntity = await _apiKeyRepository.FindByPrefixAsync(prefix, Context.RequestAborted);
        if (apiKeyEntity == null)
            return AuthenticateResult.Fail($"Invalid API key: Prefix '{prefix}' not found.");

        if (!ApiKeyManager.VerifyHash(secret, apiKeyEntity.KeyHash))
            return AuthenticateResult.Fail("Invalid API key: Secret mismatch.");

        if (apiKeyEntity.ExpiresAt.HasValue && apiKeyEntity.ExpiresAt.Value < DateTime.UtcNow)
            return AuthenticateResult.Fail("API key has expired.");

        if (!apiKeyEntity.IsActive)
            return AuthenticateResult.Fail("API key is inactive.");

        var claims = new List<System.Security.Claims.Claim>
        {
            new System.Security.Claims.Claim(AbpClaimTypes.UserId, apiKeyEntity.UserId.ToString()),
            new System.Security.Claims.Claim(AbpClaimTypes.TenantId, apiKeyEntity.TenantId?.ToString() ?? string.Empty),
            new System.Security.Claims.Claim(ApiKeyConstants.ApiKeyIdClaimType, apiKeyEntity.Id.ToString())
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);
        return AuthenticateResult.Success(ticket);
    }

    private string? ResolveApiKey()
    {
        if (Request.Headers.TryGetValue(Options.HeaderName, out var headerValue) &&
            !string.IsNullOrWhiteSpace(headerValue))
            return headerValue.ToString().Trim();

        if (Request.Headers.TryGetValue(ApiKeyConstants.HeaderNameAlternate, out var altValue) &&
            !string.IsNullOrWhiteSpace(altValue))
            return altValue.ToString().Trim();

        foreach (var paramName in ApiKeyConstants.QueryParameterNames)
        {
            if (Request.Query.TryGetValue(paramName, out var queryValue) &&
                !string.IsNullOrWhiteSpace(queryValue))
                return queryValue.ToString().Trim();
        }

        return null;
    }
}
