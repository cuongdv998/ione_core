using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Users;

namespace iOne.Workflow;

public class ElsaWorkflowService : IElsaWorkflowService, ITransientDependency
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ElsaWorkflowOptions _options;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<ElsaWorkflowService> _logger;
    private readonly ICurrentUser _currentUser;

    public ElsaWorkflowService(
        IHttpClientFactory httpClientFactory,
        IOptions<ElsaWorkflowOptions> options,
        IHttpContextAccessor httpContextAccessor,
        ILogger<ElsaWorkflowService> logger,
        ICurrentUser currentUser)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
        _currentUser = currentUser;
    }

    public async Task InitTerminatePolicyRequestAsync(Guid policyVersionId, DateTime terminationDate, decimal totalRefundAmount, CancellationToken cancellationToken = default)
    {
        var accessToken = await GetElsaAccessTokenAsync(cancellationToken);

        var authorization = _httpContextAccessor.HttpContext?.Request?.Headers["Authorization"].ToString();
        if (string.IsNullOrEmpty(authorization))
        {
            _logger.LogWarning("Elsa workflow call skipped: no Authorization header in current request. PolicyVersionId={PolicyVersionId}", policyVersionId);
            throw new InvalidOperationException("Cannot initiate Elsa workflow: no bearer token in current request.");
        }

        var authToken = authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? authorization.Substring(7)
            : authorization;

        var workflowInput = new
        {
            authToken,
            businessKey = policyVersionId.ToString(),
            reporterId = _currentUser.Id,
            terminationDate,
            totalRefundAmount
        };

        await DispatchWorkflowAsync(_options.TerminatePolicyWorkflowId, accessToken, workflowInput, cancellationToken);
    }

    public async Task InitCreatePolicyWorkflowAsync(Guid policyVersionId, string? approverId = null, bool useMotorbikeCreatePolicyWorkflow = false, string? approvalBusinessCode = null, CancellationToken cancellationToken = default)
    {
        var accessToken = await GetElsaAccessTokenAsync(cancellationToken);

        var authorization = _httpContextAccessor.HttpContext?.Request?.Headers["Authorization"].ToString();
        if (string.IsNullOrEmpty(authorization))
        {
            _logger.LogWarning("Elsa workflow call skipped: no Authorization header in current request. PolicyVersionId={PolicyVersionId}", policyVersionId);
            authorization = "";
        }

        var authToken = authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? authorization.Substring(7)
            : authorization;

        var workflowId = useMotorbikeCreatePolicyWorkflow
            ? (_options.CreatePolicyMotorbikeWorkflowId ?? string.Empty).Trim()
            : (_options.CreatePolicyWorkflowId ?? string.Empty).Trim();

        if (useMotorbikeCreatePolicyWorkflow && string.IsNullOrEmpty(workflowId))
        {
            _logger.LogError("Elsa motorbike create-policy workflow id is not configured. PolicyVersionId={PolicyVersionId}", policyVersionId);
            throw new InvalidOperationException(
                $"Elsa workflow id is not configured: {nameof(ElsaWorkflowOptions.CreatePolicyMotorbikeWorkflowId)}.");
        }

        object workflowInput;
        if (string.IsNullOrWhiteSpace(approvalBusinessCode))
        {
            workflowInput = new
            {
                authToken,
                businessKey = policyVersionId.ToString(),
                reporterId = _currentUser.Id,
                assigneeId = approverId ?? ""
            };
        }
        else
        {
            workflowInput = new
            {
                authToken,
                businessKey = policyVersionId.ToString(),
                reporterId = _currentUser.Id,
                assigneeId = approverId ?? "",
                approvalBusinessCode = approvalBusinessCode.Trim()
            };
        }

        await DispatchWorkflowAsync(workflowId, accessToken, workflowInput, cancellationToken);
    }

    public async Task InitCreateClaimWorkflowAsync(Guid claimId, string? assigneeId = null, CancellationToken cancellationToken = default)
    {
        var accessToken = await GetElsaAccessTokenAsync(cancellationToken);

        var authorization = _httpContextAccessor.HttpContext?.Request?.Headers["Authorization"].ToString();
        if (string.IsNullOrEmpty(authorization))
        {
            _logger.LogWarning("Elsa workflow call skipped: no Authorization header in current request. ClaimId={ClaimId}", claimId);
            throw new InvalidOperationException("Cannot initiate Elsa workflow: no bearer token in current request.");
        }

        var authToken = authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? authorization.Substring(7)
            : authorization;

        var workflowInput = new
        {
            authToken,
            businessKey = claimId.ToString(),
            reporterId = _currentUser.Id,
            assigneeId = assigneeId ?? ""
        };

        await DispatchWorkflowAsync(_options.CreateClaimWorkflowId, accessToken, workflowInput, cancellationToken);
    }

    public async Task InitCreateClaimFolderWorkflowAsync(
        Guid claimFolderId,
        string? assigneeId = null,
        Guid? assigneeOrganizationId = null,
        DateTime? assessmentStartDate = null,
        CancellationToken cancellationToken = default)
    {
        var accessToken = await GetElsaAccessTokenAsync(cancellationToken);

        var authorization = _httpContextAccessor.HttpContext?.Request?.Headers["Authorization"].ToString();
        if (string.IsNullOrEmpty(authorization))
        {
            _logger.LogWarning("Elsa workflow call skipped: no Authorization header in current request. ClaimFolderId={ClaimFolderId}", claimFolderId);
            throw new InvalidOperationException("Cannot initiate Elsa workflow: no bearer token in current request.");
        }

        var authToken = authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? authorization.Substring(7)
            : authorization;

        var workflowInput = new
        {
            authToken,
            businessKey = claimFolderId.ToString(),
            reporterId = _currentUser.Id,
            assigneeId = assigneeId ?? "",
            assigneeOrganizationId = assigneeOrganizationId?.ToString() ?? "",
            assessmentStartDate
        };

        await DispatchWorkflowAsync(_options.CreateClaimFolderWorkflowId, accessToken, workflowInput, cancellationToken);
    }

    public async Task InitCreateRepairPlanWorkflowAsync(Guid businessKey, string? assigneeId = null, CancellationToken cancellationToken = default)
    {
        var accessToken = await GetElsaAccessTokenAsync(cancellationToken);

        var authorization = _httpContextAccessor.HttpContext?.Request?.Headers["Authorization"].ToString();
        if (string.IsNullOrEmpty(authorization))
        {
            _logger.LogWarning("Elsa workflow call skipped: no Authorization header in current request. BusinessKey={BusinessKey}", businessKey);
            throw new InvalidOperationException("Cannot initiate Elsa workflow: no bearer token in current request.");
        }

        var authToken = authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? authorization.Substring(7)
            : authorization;

        var workflowInput = new
        {
            authToken,
            businessKey = businessKey.ToString(),
            reporterId = _currentUser.Id,
            assigneeId = assigneeId ?? ""
        };

        await DispatchWorkflowAsync(_options.CreateRepairPlanWorkflowId, accessToken, workflowInput, cancellationToken);
    }

    public async Task InitEndorsementWorkflowAsync(Guid policyVersionId, string? approverId = null, CancellationToken cancellationToken = default)
    {
        var accessToken = await GetElsaAccessTokenAsync(cancellationToken);

        var authorization = _httpContextAccessor.HttpContext?.Request?.Headers["Authorization"].ToString();
        if (string.IsNullOrEmpty(authorization))
        {
            _logger.LogWarning("Elsa workflow call skipped: no Authorization header in current request. PolicyVersionId={PolicyVersionId}", policyVersionId);
            throw new InvalidOperationException("Cannot initiate Elsa workflow: no bearer token in current request.");
        }

        var authToken = authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? authorization.Substring(7)
            : authorization;

        var workflowInput = new
        {
            authToken,
            businessKey = policyVersionId.ToString(),
            reporterId = _currentUser.Id,
            assigneeId = approverId ?? ""
        };

        await DispatchWorkflowAsync(_options.EndorsementWorkflowId, accessToken, workflowInput, cancellationToken);
    }

    public async Task InitOnsiteAssessmentWorkflowAsync(Guid claimId, Guid assigneeOrganizationId, Guid assigneeId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        var accessToken = await GetElsaAccessTokenAsync(cancellationToken);

        var authorization = _httpContextAccessor.HttpContext?.Request?.Headers["Authorization"].ToString();
        if (string.IsNullOrEmpty(authorization))
        {
            _logger.LogWarning("Elsa workflow call skipped: no Authorization header in current request. ClaimId={ClaimId}", claimId);
            throw new InvalidOperationException("Cannot initiate Elsa workflow: no bearer token in current request.");
        }

        var authToken = authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? authorization.Substring(7)
            : authorization;

        var workflowInput = new
        {
            authToken,
            businessKey = claimId.ToString(),
            reporterId = _currentUser.Id,
            assigneeId = assigneeId.ToString(),
            assigneeOrganizationId = assigneeOrganizationId.ToString(),
            startDate,
            endDate
        };

        await DispatchWorkflowAsync(_options.OnsiteAssessmentWorkflowId, accessToken, workflowInput, cancellationToken);
    }

    public async Task TriggerApprovalAsync(string eventName, string workflowInstanceId, string action, string? assigneeId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Triggering Elsa approval workflow. EventName={EventName}, WorkflowInstanceId={WorkflowInstanceId}, Action={Action}", eventName, workflowInstanceId, action);
        var accessToken = await GetElsaAccessTokenAsync(cancellationToken);
        await TriggerEventAsync(eventName, action, workflowInstanceId, accessToken, assigneeId, cancellationToken);
        _logger.LogInformation("Elsa approval workflow trigger completed. EventName={EventName}, Action={Action}", eventName, action);
    }

    public async Task InitSendNotifyWorkflowAsync(List<Guid> notifyIds, CancellationToken cancellationToken = default)
    {
        if (notifyIds == null || notifyIds.Count == 0)
        {
            _logger.LogWarning("Elsa SendNotify workflow call skipped: no notification IDs provided.");
            return;
        }

        var accessToken = await GetElsaAccessTokenAsync(cancellationToken);

        var authorization = _httpContextAccessor.HttpContext?.Request?.Headers["Authorization"].ToString();
        if (string.IsNullOrEmpty(authorization))
        {
            _logger.LogWarning("Elsa workflow call skipped: no Authorization header in current request. NotifyIds count={Count}", notifyIds.Count);
            throw new InvalidOperationException("Cannot initiate Elsa workflow: no bearer token in current request.");
        }

        var authToken = authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? authorization.Substring(7)
            : authorization;

        var workflowInput = new
        {
            authToken,
            notifyIds = notifyIds.Select(id => id.ToString()).ToList(),
            reporterId = _currentUser.Id
        };

        await DispatchWorkflowAsync(_options.SendNotifyWorkflowId, accessToken, workflowInput, cancellationToken);
    }

    private async Task TriggerEventAsync(string eventName, string action, string? workflowInstanceId, string accessToken, string? assigneeId, CancellationToken cancellationToken = default)
    {
        var baseUrl = _options.BaseUrl.TrimEnd('/');
        var url = $"{baseUrl}/elsa/api/events/{Uri.EscapeDataString(eventName)}/trigger";

        var client = _httpClientFactory.CreateClient("Elsa");

        var requestBody = new
        {
            input = new { action, assigneeId },
            workflowInstanceId
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {accessToken}");
        request.Content = JsonContent.Create(requestBody);

        _logger.LogInformation("Triggering Elsa event. EventName={EventName}, Url={Url}", eventName, url);

        var response = await client.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Elsa event trigger failed. EventName={EventName}, StatusCode={StatusCode}, ReasonPhrase={ReasonPhrase}, Content={Content}",
                eventName, response.StatusCode, response.ReasonPhrase, errorContent);
            throw new InvalidOperationException($"Elsa event trigger failed: {response.StatusCode} - {response.ReasonPhrase}");
        }

        _logger.LogInformation("Elsa event triggered successfully. EventName={EventName}", eventName);
    }

    private async Task<string> GetElsaAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        var baseUrl = _options.BaseUrl.TrimEnd('/');
        var url = $"{baseUrl}/elsa/api/identity/login";

        var client = _httpClientFactory.CreateClient("Elsa");

        var loginRequest = new ElsaLoginRequest
        {
            Username = _options.Username,
            Password = _options.Password
        };

        _logger.LogInformation("Authenticating with Elsa at {Url}", url);

        var response = await client.PostAsJsonAsync(url, loginRequest, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Elsa authentication failed. StatusCode={StatusCode}, ReasonPhrase={ReasonPhrase}, Content={Content}",
                response.StatusCode, response.ReasonPhrase, errorContent);
            throw new InvalidOperationException($"Elsa authentication failed: {response.StatusCode} - {response.ReasonPhrase}");
        }

        var loginResponse = await response.Content.ReadFromJsonAsync<ElsaLoginResponse>(cancellationToken);

        if (loginResponse == null || !loginResponse.IsAuthenticated || string.IsNullOrEmpty(loginResponse.AccessToken))
        {
            _logger.LogError("Elsa authentication failed: invalid response or not authenticated");
            throw new InvalidOperationException("Elsa authentication failed: invalid response or not authenticated");
        }

        _logger.LogInformation("Elsa authentication successful");
        return loginResponse.AccessToken;
    }

    private async Task DispatchWorkflowAsync(string workflowId, string accessToken, object? input = null, CancellationToken cancellationToken = default)
    {
        var baseUrl = _options.BaseUrl.TrimEnd('/');
        var url = $"{baseUrl}/elsa/api/workflow-definitions/{workflowId}/dispatch";

        var client = _httpClientFactory.CreateClient("Elsa");

        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {accessToken}");
        var requestBody = input != null ? new { input } : new object();
        request.Content = JsonContent.Create(requestBody);

        _logger.LogInformation("Dispatching Elsa workflow. WorkflowId={WorkflowId}, Url={Url}", workflowId, url);

        var response = await client.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Elsa workflow dispatch failed. WorkflowId={WorkflowId}, StatusCode={StatusCode}, ReasonPhrase={ReasonPhrase}, Content={Content}",
                workflowId, response.StatusCode, response.ReasonPhrase, errorContent);
            throw new InvalidOperationException($"Elsa workflow dispatch failed: {response.StatusCode} - {response.ReasonPhrase}");
        }

        _logger.LogInformation("Elsa workflow dispatched successfully. WorkflowId={WorkflowId}", workflowId);
    }

    private class ElsaLoginRequest
    {
        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;

        [JsonPropertyName("password")]
        public string Password { get; set; } = string.Empty;
    }

    private class ElsaLoginResponse
    {
        [JsonPropertyName("isAuthenticated")]
        public bool IsAuthenticated { get; set; }

        [JsonPropertyName("accessToken")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("refreshToken")]
        public string RefreshToken { get; set; } = string.Empty;
    }

}
