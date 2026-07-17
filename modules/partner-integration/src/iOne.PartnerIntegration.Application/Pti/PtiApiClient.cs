using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using iOne.PartnerIntegration.Pti.Crypto;
using iOne.PartnerIntegration.Pti.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace iOne.PartnerIntegration.Pti;

public class PtiApiClient : IPtiApiClient, ITransientDependency
{
    private const string LoginPath = "authentication/login/v2/jwt";
    private const string VehicleCreatePath = "vehicle/create";
    private const string VehicleLookupInfoPath = "vehicle/lookup-info";

    private readonly HttpClient _httpClient;
    private readonly IPtiConfigProvider _configProvider;
    private readonly IHostEnvironment? _env;
    private readonly ILogger<PtiApiClient> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };

    public PtiApiClient(
        HttpClient httpClient,
        IPtiConfigProvider configProvider,
        ILogger<PtiApiClient> logger,
        IHostEnvironment? env = null)
    {
        _httpClient = httpClient;
        _configProvider = configProvider;
        _env = env;
        _logger = logger;
    }

    /// <summary>
    /// Resolves a key path: if the path is relative, it's anchored to the content root
    /// (the folder containing the running assembly / Docker WORKDIR), so
    /// "Keys/public_key_pti_ibds_uat.asc" works in both local dev and containers.
    /// </summary>
    private string ResolveKeyPath(string configured)
    {
        if (string.IsNullOrWhiteSpace(configured))
            return configured;
        if (Path.IsPathRooted(configured))
            return configured;

        var contentRoot = _env?.ContentRootPath
            ?? AppContext.BaseDirectory;
        return Path.Combine(contentRoot, configured);
    }

    public async Task<PtiCreateVehicleResponse> IssueVehicleAsync(
        PtiCreateVehicleRequest request, CancellationToken cancellationToken = default)
    {
        var options = await _configProvider.GetOptionsAsync(cancellationToken);
        var token = await AcquireTokenAsync(options, cancellationToken);

        var baseUrl = options.BaseUrl.TrimEnd('/');
        var uri = new Uri($"{baseUrl}/{VehicleCreatePath}");

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, uri);
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ExtractBearerValue(token));
        httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var bodyJson = JsonSerializer.Serialize(request, JsonOptions);
        _logger.LogInformation("PTI vehicle/create request: {Body}", bodyJson);

        string requestContent;
        if (options.EnableSigning
            && string.Equals(options.SignatureMode, "EncryptBody", StringComparison.OrdinalIgnoreCase))
        {
            var passphrase = options.PgpEncryptBodyResponsePassphrase ?? string.Empty;
            var armoredBody = PtiRsaSigner.EncryptAndSignWithPgpFromArmored(
                bodyJson,
                PtiIbdsPublicKey.IbdsUatPgpPublicKeyArmored,
                PtiIoneIbdsPrivateKey.IbdsPartnerPgpPrivateKeyArmored,
                passphrase);
            requestContent = armoredBody;
            httpRequest.Content = new StringContent(armoredBody, Encoding.UTF8, "text/plain");
            _logger.LogInformation("PTI vehicle/create request encrypted+signed (armored PGP): {BodyPreview}",
                armoredBody.Length > 200 ? armoredBody.Substring(0, 200) + "..." : armoredBody);
        }
        else if (options.EnableSigning
            && string.Equals(options.SignatureMode, "Header", StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrWhiteSpace(options.PrivateKeyPath))
        {
            var resolvedKeyPath = ResolveKeyPath(options.PrivateKeyPath);
            var signature = PtiRsaSigner.Sign(bodyJson, resolvedKeyPath);
            httpRequest.Headers.TryAddWithoutValidation("Signature", signature);
            requestContent = bodyJson;
            httpRequest.Content = new StringContent(bodyJson, Encoding.UTF8, "application/json");
        }
        else
        {
            if (!options.EnableSigning)
                _logger.LogWarning("PTI signing is disabled (ENABLESIGNING=false) — sending plain JSON body.");

            requestContent = bodyJson;
            httpRequest.Content = new StringContent(bodyJson, Encoding.UTF8, "application/json");
        }

        var httpResponse = await _httpClient.SendAsync(httpRequest, cancellationToken);
        _logger.LogInformation("PTI vehicle/create response: {Response}", httpResponse);
        var rawResponseBody = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

        string auditResponseBody;
        try
        {
                _logger.LogInformation("PTI vehicle/create response raw: {Body}", rawResponseBody);
                auditResponseBody = NeedsPgpDecryptForVehicleCreateResponse(options)
                ? DecryptVehicleCreateEncryptedResponse(rawResponseBody, options)
                : rawResponseBody;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex,
                "PTI vehicle/create response decryption failed StatusCode={StatusCode}",
                (int)httpResponse.StatusCode);
            throw CreateResponseDecryptBusinessException(ex);
        }

        _logger.LogInformation(
            "PTI vehicle/create response StatusCode={StatusCode}, Body={Body}",
            (int)httpResponse.StatusCode, auditResponseBody);

        if (!httpResponse.IsSuccessStatusCode)
        {
            var errorMsg = TryExtractPtiErrorMessage(auditResponseBody);
            throw new PtiPartnerHttpException(
                "Policy:Partner:PtiIssueFailed",
                (int)httpResponse.StatusCode,
                auditResponseBody,
                errorMsg,
                uri.ToString());
        }

        var result = JsonSerializer.Deserialize<PtiCreateVehicleResponse>(auditResponseBody, JsonOptions);
        if (result == null)
        {
            throw new BusinessException("Policy:Partner:PtiIssueFailed")
                .WithData("Error", "PTI returned an empty response");
        }

        return result;
    }

    public async Task<(string Url, string? RawResponseJson, int HttpStatus)> LookupVehicleInfoAsync(
        string transId, CancellationToken cancellationToken = default)
    {
        var options = await _configProvider.GetOptionsAsync(cancellationToken);
        var token = await AcquireTokenAsync(options, cancellationToken);

        var baseUrl = options.BaseUrl.TrimEnd('/');
        var encodedTransId = Uri.EscapeDataString(transId);
        var url = $"{baseUrl}/{VehicleLookupInfoPath}?page=0&size=10&filter=transId_ss%3A{encodedTransId}";
        var uri = new Uri(url);

        using var httpRequest = new HttpRequestMessage(HttpMethod.Get, uri);
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ExtractBearerValue(token));
        httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpRequest.Headers.TryAddWithoutValidation("Content-Type", "application/json");

        _logger.LogInformation("PTI vehicle/lookup-info request: transId={TransId}", transId);

        var httpResponse = await _httpClient.SendAsync(httpRequest, cancellationToken);
        var rawBody = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
        var statusCode = (int)httpResponse.StatusCode;

        _logger.LogInformation(
            "PTI vehicle/lookup-info response StatusCode={StatusCode}, Body={Body}",
            statusCode, rawBody);

        if (!httpResponse.IsSuccessStatusCode)
        {
            var errorMsg = TryExtractPtiErrorMessage(rawBody);
            throw new PtiPartnerHttpException(
                "Policy:Partner:PtiInquiryFailed",
                statusCode,
                rawBody,
                errorMsg,
                url);
        }

        return (url, rawBody, statusCode);
    }

    private async Task<string> AcquireTokenAsync(PtiOptions options, CancellationToken cancellationToken)
    {
        var authBase = options.AuthBaseUrl.TrimEnd('/');
        var uri = new Uri($"{authBase}/{LoginPath}");

        var loginBody = JsonSerializer.Serialize(new
        {
            userName = options.UserName,
            password = options.Password,
            partnerName = options.PartnerName
        });

        using var request = new HttpRequestMessage(HttpMethod.Post, uri);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Content = new StringContent(loginBody, Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request, cancellationToken);
        var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new BusinessException("Policy:Partner:PtiAuthFailed")
                .WithData("StatusCode", (int)response.StatusCode)
                .WithData("Error", responseJson);
        }

        var tokenResponse = JsonSerializer.Deserialize<PtiTokenResponse>(responseJson, JsonOptions);
        if (tokenResponse == null || !tokenResponse.IsSuccess)
        {
            throw new BusinessException("Policy:Partner:PtiAuthFailed")
                .WithData("StatusCode", (object?)tokenResponse?.StatusCode ?? "null")
                .WithData("Error", tokenResponse?.Message ?? "Empty token response");
        }

        return tokenResponse.Token!;
    }

    /// <summary>
    /// PTI Bearer token value may come back as "Bearer eyJ..." — strip the prefix if present.
    /// </summary>
    private static string ExtractBearerValue(string token)
    {
        if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return token.Substring(7).Trim();
        return token.Trim();
    }

    private static bool NeedsPgpDecryptForVehicleCreateResponse(PtiOptions options)
        => options.EnableSigning
           && string.Equals(options.SignatureMode, "EncryptBody", StringComparison.OrdinalIgnoreCase);

    private static string DecryptVehicleCreateEncryptedResponse(string rawWire, PtiOptions options)
        => PtiRsaSigner.DecryptOpenPgpDirectToUtf8(
            rawWire,
            PtiIoneIbdsPrivateKey.IbdsPartnerPgpPrivateKeyArmored,
            string.IsNullOrEmpty(options.PgpEncryptBodyResponsePassphrase)
                ? null
                : Encoding.UTF8.GetBytes(options.PgpEncryptBodyResponsePassphrase));

    private static BusinessException CreateResponseDecryptBusinessException(InvalidOperationException cause)
        => new BusinessException("Policy:Partner:PtiIssueFailed")
            .WithData("Error", "PTI EncryptBody response could not be decrypted")
            .WithData("Cause", cause.Message);

    private static string? TryExtractPtiErrorMessage(string json)
    {
        try
        {
            var err = JsonSerializer.Deserialize<PtiErrorResponse>(json, JsonOptions);
            if (err?.Message != null)
                return $"{err.Code} - {err.Message}";
        }
        catch
        {
            // ignore parse errors
        }
        return null;
    }
}
