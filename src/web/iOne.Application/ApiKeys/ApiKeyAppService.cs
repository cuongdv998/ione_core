using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace iOne.ApiKeys;

/// <summary>
/// API key management. Only Bearer (user login) is allowed; API keys cannot be used to manage keys.
/// </summary>
[Authorize(AuthenticationSchemes = "OpenIddict.Validation.AspNetCore")]
public class ApiKeyAppService : iOne.iOneAppService, IApiKeyAppService
{
    private readonly ApiKeyManager _apiKeyManager;
    private readonly IApiKeyRepository _apiKeyRepository;

    public ApiKeyAppService(ApiKeyManager apiKeyManager, IApiKeyRepository apiKeyRepository)
    {
        _apiKeyManager = apiKeyManager;
        _apiKeyRepository = apiKeyRepository;
    }

    public async Task<CreateApiKeyResultDto> CreateAsync(CreateApiKeyDto input)
    {
        var userId = CurrentUser.Id ?? throw new UnauthorizedAccessException("User must be logged in to create an API key.");
        var (entity, fullKey) = await _apiKeyManager.CreateAsync(userId, input.Name, input.ExpiresAt);
        return new CreateApiKeyResultDto
        {
            Id = entity.Id,
            Name = entity.Name,
            FullKey = fullKey
        };
    }

    public async Task<List<ApiKeyDto>> GetListAsync()
    {
        var userId = CurrentUser.Id ?? throw new UnauthorizedAccessException("User must be logged in to list API keys.");
        var list = await _apiKeyRepository.GetListByUserIdAsync(userId);
        return list.Select(e => new ApiKeyDto
        {
            Id = e.Id,
            Name = e.Name,
            Prefix = e.Prefix,
            ExpiresAt = e.ExpiresAt,
            IsActive = e.IsActive,
            CreationTime = e.CreationTime
        }).ToList();
    }

    public async Task RevokeAsync(Guid id)
    {
        var userId = CurrentUser.Id ?? throw new UnauthorizedAccessException("User must be logged in to revoke an API key.");
        var apiKey = await _apiKeyRepository.GetAsync(id);
        if (apiKey.UserId != userId)
            throw new UnauthorizedAccessException("You can only revoke your own API keys.");
        await _apiKeyManager.RevokeAsync(apiKey);
    }
}
