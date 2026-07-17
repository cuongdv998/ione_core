using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;

namespace iOne.WebviewAuth;

[Dependency(ServiceLifetime.Transient, ReplaceServices = true)]
[ExposeServices(typeof(IPartnerWebviewTokenValidator))]
public class AmioTokenValidator : IPartnerWebviewTokenValidator, ITransientDependency
{
    private const string ClientName = "AmioTokenVerificationClient";
    private const string VerifyTokenPath = "/cust/partner-api/v1/verify-token";
    private static readonly Regex SensitiveValueRegex = new(
        "\"(token|access_token|refresh_token|apiKey|authorization)\"\\s*:\\s*\"[^\"]*\"",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AmioTokenValidator> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AmioTokenValidator(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<AmioTokenValidator> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<PartnerWebviewTokenValidationResult> ValidateAsync(string partnerCode, string partnerToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(partnerToken))
        {
            _logger.LogWarning("Amio token validation skipped because partner token is empty. PartnerCode={PartnerCode}", partnerCode);
            return PartnerWebviewTokenValidationResult.Failed();
        }

        var baseUrl = _configuration["WebviewAuth:PartnerVerification:BaseUrl"];
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            _logger.LogWarning("Amio token validation failed because WebviewAuth:PartnerVerification:BaseUrl is not configured.");
            return PartnerWebviewTokenValidationResult.Failed();
        }

        var requestUri = new Uri(new Uri(baseUrl.TrimEnd('/')), VerifyTokenPath);
        var requestId = _httpContextAccessor.HttpContext?.TraceIdentifier ?? "n/a";
        var client = _httpClientFactory.CreateClient(ClientName);

        using var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", partnerToken);

        _logger.LogInformation(
            "Sending Amio verify-token request. PartnerCode={PartnerCode}, RequestId={RequestId}, Method={Method}, Url={Url}, TokenMask={TokenMask}",
            partnerCode,
            requestId,
            request.Method.Method,
            requestUri,
            MaskToken(partnerToken));

        var stopwatch = Stopwatch.StartNew();

        try
        {
            using var response = await client.SendAsync(request, cancellationToken);
            var responseBody = response.Content == null
                ? string.Empty
                : await response.Content.ReadAsStringAsync(cancellationToken);
            stopwatch.Stop();

            _logger.LogInformation(
                "Received Amio verify-token response. PartnerCode={PartnerCode}, RequestId={RequestId}, StatusCode={StatusCode}, DurationMs={DurationMs}, ResponseBody={ResponseBody}",
                partnerCode,
                requestId,
                (int)response.StatusCode,
                stopwatch.ElapsedMilliseconds,
                SanitizeResponseBody(responseBody));

            if (!response.IsSuccessStatusCode)
            {
                return PartnerWebviewTokenValidationResult.Failed();
            }

            try
            {
                var amioResponse = JsonSerializer.Deserialize<AmioVerifyTokenResponse>(responseBody);
                if (amioResponse == null || amioResponse.ResponseCode != "00")
                {
                    _logger.LogWarning("Amio verify-token response indicates failure. ResponseCode={ResponseCode}, Message={Message}", 
                        amioResponse?.ResponseCode, amioResponse?.Message);
                    return PartnerWebviewTokenValidationResult.Failed();
                }

                return new PartnerWebviewTokenValidationResult
                {
                    Success = true,
                    CustomerCode = amioResponse.Data?.CustomerCode,
                    CustomerName = amioResponse.Data?.CustomerName,
                    CustomerPhone = amioResponse.Data?.CustomerPhone,
                    CustomerEmail = amioResponse.Data?.CustomerEmail,
                    CustomerAddress = amioResponse.Data?.CustomerAddress
                };
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize Amio verify-token response.");
                return PartnerWebviewTokenValidationResult.Failed();
            }
        }
        catch (Exception ex) when (ex is TaskCanceledException || ex is HttpRequestException)
        {
            stopwatch.Stop();
            _logger.LogWarning(
                ex,
                "Amio verify-token request failed. PartnerCode={PartnerCode}, RequestId={RequestId}, DurationMs={DurationMs}, Url={Url}",
                partnerCode,
                requestId,
                stopwatch.ElapsedMilliseconds,
                requestUri);
            return PartnerWebviewTokenValidationResult.Failed();
        }
    }

    public async Task<object> SendOrderResultAsync(SendOrderResultInputDto input, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(input.CertificateUrl))
        {
            _logger.LogWarning("Amio order-result skipped because certificate URL is empty.");
            if (_httpContextAccessor.HttpContext != null) _httpContextAccessor.HttpContext.Response.StatusCode = 400;
            return new { responseCode = "400", message = "Certificate URL is empty" };
        }

        var baseUrl = _configuration["WebviewAuth:PartnerVerification:BaseUrl"];
        var apiKey = _configuration["WebviewAuth:PartnerVerification:ApiKey"];

        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            _logger.LogWarning("Amio order-result failed because WebviewAuth:PartnerVerification:BaseUrl is not configured.");
            if (_httpContextAccessor.HttpContext != null) _httpContextAccessor.HttpContext.Response.StatusCode = 500;
            return new { responseCode = "500", message = "Configuration error" };
        }

        var requestUri = new Uri(new Uri(baseUrl.TrimEnd('/')), "/itgr/partner-api/v1/insurance/order-result");
        var requestId = _httpContextAccessor.HttpContext?.TraceIdentifier ?? "n/a";
        var client = _httpClientFactory.CreateClient(ClientName);

        using var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            request.Headers.Add("x-api-key", apiKey);
        }

        var body = new
        {
            transId = input.TransId,
            @event = "ORDER_SUCCESS",
            data = new
            {
                campaignCode = "ANPHARE_INSURANCE",
                orderId = input.OrderId,
                status = "success",
                product = input.Product,
                amount = input.Amount,
                customerCode = input.CustomerCode,
                insurance_cert_url = input.CertificateUrl,
                contractNumber = input.ContractNumber,
                vehicleOwner = input.VehicleOwner,
                licensePlate = input.LicensePlate,
                effectivePeriod = input.EffectivePeriod
            }
        };
        request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

        _logger.LogInformation(
            "Sending Amio order-result request. RequestId={RequestId}, Method={Method}, Url={Url}, Payload={Payload}",
            requestId,
            request.Method.Method,
            requestUri,
            JsonSerializer.Serialize(body));

        var stopwatch = Stopwatch.StartNew();

        try
        {
            using var response = await client.SendAsync(request, cancellationToken);
            var responseBody = response.Content == null
                ? string.Empty
                : await response.Content.ReadAsStringAsync(cancellationToken);
            stopwatch.Stop();

            _logger.LogInformation(
                "Received Amio order-result response. RequestId={RequestId}, StatusCode={StatusCode}, DurationMs={DurationMs}, ResponseBody={ResponseBody}",
                requestId,
                (int)response.StatusCode,
                stopwatch.ElapsedMilliseconds,
                SanitizeResponseBody(responseBody));

            if (_httpContextAccessor.HttpContext != null)
            {
                _httpContextAccessor.HttpContext.Response.StatusCode = (int)response.StatusCode;
            }

            if (!string.IsNullOrWhiteSpace(responseBody))
            {
                try
                {
                    return JsonSerializer.Deserialize<object>(responseBody) ?? new { };
                }
                catch
                {
                    return responseBody;
                }
            }

            return new { };
        }
        catch (Exception ex) when (ex is TaskCanceledException || ex is HttpRequestException)
        {
            stopwatch.Stop();
            _logger.LogWarning(
                ex,
                "Amio order-result request failed. RequestId={RequestId}, DurationMs={DurationMs}, Url={Url}",
                requestId,
                stopwatch.ElapsedMilliseconds,
                requestUri);

            if (_httpContextAccessor.HttpContext != null)
            {
                _httpContextAccessor.HttpContext.Response.StatusCode = 500;
            }

            return new { responseCode = "500", message = ex.Message };
        }
    }

    private static string MaskToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return "<empty>";
        }

        return token.Length <= 8
            ? "***"
            : $"{token[..4]}...{token[^4..]}";
    }

    private static string SanitizeResponseBody(string responseBody)
    {
        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return "<empty>";
        }

        string sanitized;
        try
        {
            using var json = JsonDocument.Parse(responseBody);
            sanitized = JsonSerializer.Serialize(json.RootElement);
        }
        catch
        {
            sanitized = responseBody;
        }

        sanitized = SensitiveValueRegex.Replace(sanitized, "\"$1\":\"***\"");
        return sanitized.Length > 1000 ? $"{sanitized[..1000]}...(truncated)" : sanitized;
    }

    private class AmioVerifyTokenResponse
    {
        [JsonPropertyName("responseCode")]
        public string ResponseCode { get; set; } = null!;

        [JsonPropertyName("message")]
        public string Message { get; set; } = null!;

        [JsonPropertyName("data")]
        public AmioCustomerData? Data { get; set; }
    }

    private class AmioCustomerData
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
}
