using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;

namespace iOne.PartnerIntegration.Vni;

public class VniApiClient : IVniApiClient, ITransientDependency
{
    private const string CivilLiabilityInsertPath = "OmniChannel/Contract/Connect/Car/CivilLiability/Insert";
    private const string CertificateInsertPath = "OmniChannel/Contract/Connect/Car/Certificate/Insert";

    private readonly HttpClient _httpClient;
    private readonly IVniConfigProvider _configProvider;
    private readonly ILogger<VniApiClient> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };

    private static readonly JsonSerializerOptions LogJsonOptions = new()
    {
        WriteIndented = true
    };

    public VniApiClient(HttpClient httpClient, IVniConfigProvider configProvider, ILogger<VniApiClient> logger)
    {
        _httpClient = httpClient;
        _configProvider = configProvider;
        _logger = logger;
    }

    public async Task<VniIssueResult> IssueCarTndsPolicyAsync(VniCarTndsIssueRequest request, CancellationToken cancellationToken = default)
    {
        var options = await _configProvider.GetOptionsAsync(cancellationToken);
        request.DviSl = options.DviSl;
        request.MaCn = options.MaCn;

        var requestJson = JsonSerializer.Serialize(request, LogJsonOptions);
        _logger.LogInformation("VNI Car TNDS Issue request to {Path}: {Request}", CivilLiabilityInsertPath, requestJson);

        var response = await PostJsonAsync(options, CivilLiabilityInsertPath, request, cancellationToken);
        var result = await ReadResultAsync(response, cancellationToken);

        var responseJson = JsonSerializer.Serialize(new { result.Code, result.Message, result.Data, result.Total }, LogJsonOptions);
        _logger.LogInformation("VNI Car TNDS Issue response StatusCode={StatusCode}, Body: {Response}", response.StatusCode, responseJson);

        return result;
    }

    public async Task<VniIssueResult> IssueCarVcxPolicyAsync(VniCarVcxIssueRequest request, CancellationToken cancellationToken = default)
    {
        var options = await _configProvider.GetOptionsAsync(cancellationToken);
        request.DviSl = options.DviSl;
        request.MaCn = options.MaCn;

        var requestJson = JsonSerializer.Serialize(request, LogJsonOptions);
        _logger.LogInformation("VNI Car VCX Issue request to {Path}: {Request}", CertificateInsertPath, requestJson);

        var response = await PostJsonAsync(options, CertificateInsertPath, request, cancellationToken);
        var result = await ReadResultAsync(response, cancellationToken);

        var responseJson = JsonSerializer.Serialize(new { result.Code, result.Message, result.Data, result.Total }, LogJsonOptions);
        _logger.LogInformation("VNI Car VCX Issue response StatusCode={StatusCode}, Body: {Response}", response.StatusCode, responseJson);

        return result;
    }

    private async Task<HttpResponseMessage> PostJsonAsync<T>(VniOptions options, string path, T value, CancellationToken cancellationToken)
    {
        var baseUrl = options.BaseUrl.TrimEnd('/');
        var uri = new System.Uri(baseUrl + "/" + path);

        using var request = new HttpRequestMessage(HttpMethod.Post, uri);
        request.Headers.TryAddWithoutValidation("UserName", options.UserName);
        request.Headers.TryAddWithoutValidation("PassWord", options.PassWord);
        request.Headers.TryAddWithoutValidation("Channel", "API");
        request.Headers.TryAddWithoutValidation("ChannelBlock", "API");

        var json = JsonSerializer.Serialize(value, JsonOptions);
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");

        return await _httpClient.SendAsync(request, cancellationToken);
    }

    private static async Task<VniIssueResult> ReadResultAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var result = JsonSerializer.Deserialize<VniIssueResult>(json, JsonOptions);
        if (result == null)
        {
            return new VniIssueResult
            {
                Code = response.IsSuccessStatusCode ? "000" : "500",
                Message = string.IsNullOrEmpty(json) ? response.ReasonPhrase : json
            };
        }
        return result;
    }
}
