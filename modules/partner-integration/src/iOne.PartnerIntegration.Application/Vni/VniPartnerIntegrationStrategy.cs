using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using iOne.ResPartnerMessageLoggings;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;

namespace iOne.PartnerIntegration.Vni;

public class VniPartnerIntegrationStrategy : IPartnerIntegrationStrategy, ITransientDependency
{
    private const string PartnerCodeVni = "DBV";

    private readonly IVniApiClient _vniApiClient;
    private readonly VniRequestMapper _vniRequestMapper;
    private readonly IPartnerMessageLoggingService _loggingService;
    private readonly IVniConfigProvider _configProvider;
    private readonly ILogger<VniPartnerIntegrationStrategy> _logger;

    private static readonly JsonSerializerOptions LogJsonOptions = new() { WriteIndented = false };

    public VniPartnerIntegrationStrategy(
        IVniApiClient vniApiClient,
        VniRequestMapper vniRequestMapper,
        IPartnerMessageLoggingService loggingService,
        IVniConfigProvider configProvider,
        ILogger<VniPartnerIntegrationStrategy> logger)
    {
        _vniApiClient = vniApiClient;
        _vniRequestMapper = vniRequestMapper;
        _loggingService = loggingService;
        _configProvider = configProvider;
        _logger = logger;
    }

    public bool CanHandle(string partnerCode)
    {
        return string.Equals(partnerCode, PartnerCodeVni, StringComparison.OrdinalIgnoreCase);
    }

    public async Task<List<PartnerIntegrationResult>> IssuePolicyAsync(
        PartnerIssuePolicyContext context, CancellationToken cancellationToken = default)
    {
        var results = new List<PartnerIntegrationResult>();

        foreach (var product in context.Products)
        {
            var productTypeCode = product.Product?.ProductType?.Code ?? "";

            if (productTypeCode.Contains("TNDS", StringComparison.OrdinalIgnoreCase))
            {
                var result = await IssueTndsAsync(context, product, cancellationToken);
                results.Add(result);
            }

            if (productTypeCode.Contains("VCX", StringComparison.OrdinalIgnoreCase))
            {
                var result = await IssueVcxAsync(context, product, cancellationToken);
                results.Add(result);
            }
        }

        return results;
    }

    private async Task<PartnerIntegrationResult> IssueTndsAsync(
        PartnerIssuePolicyContext context,
        iOne.Policies.PolicyProduct product,
        CancellationToken cancellationToken)
    {
        var options = await _configProvider.GetOptionsAsync(cancellationToken);
        var apiUrl = $"{options.BaseUrl.TrimEnd('/')}/OmniChannel/Contract/Connect/Car/CivilLiability/Insert";
        var request = _vniRequestMapper.ToCarTndsRequest(
            context.Policy, context.CurrentVersion, product, context.FirstMotor);
        var requestJson = JsonSerializer.Serialize(request, LogJsonOptions);

        var sw = Stopwatch.StartNew();
        try
        {
            var vniResult = await _vniApiClient.IssueCarTndsPolicyAsync(request, cancellationToken);
            sw.Stop();

            var responseJson = JsonSerializer.Serialize(
                new { vniResult.Code, vniResult.Message, vniResult.Data, vniResult.Total }, LogJsonOptions);

            await LogMessageAsync(
                context.PolicyId, apiUrl, "POST", requestJson, responseJson,
                vniResult.IsSuccess ? 200 : 400, vniResult.IsSuccess,
                vniResult.IsSuccess ? null : $"{vniResult.Code} - {vniResult.Message}",
                sw.ElapsedMilliseconds, cancellationToken);

            if (!vniResult.IsSuccess)
            {
                var msg = $"VNI Car TNDS issue failed: {vniResult.Code} - {vniResult.Message}";
                _logger.LogWarning(msg);
                return new PartnerIntegrationResult
                {
                    ProductCode = "TNDS", IsSuccess = false, ErrorMessage = msg
                };
            }

            return new PartnerIntegrationResult { ProductCode = "TNDS", IsSuccess = true };
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex, "VNI Car TNDS issue API call failed for policy {PolicyId}", context.PolicyId);

            await LogMessageAsync(
                context.PolicyId, apiUrl, "POST", requestJson, null,
                null, false, ex.Message, sw.ElapsedMilliseconds, cancellationToken);

            return new PartnerIntegrationResult
            {
                ProductCode = "TNDS", IsSuccess = false, ErrorMessage = $"TNDS: {ex.Message}"
            };
        }
    }

    private async Task<PartnerIntegrationResult> IssueVcxAsync(
        PartnerIssuePolicyContext context,
        iOne.Policies.PolicyProduct product,
        CancellationToken cancellationToken)
    {
        var options = await _configProvider.GetOptionsAsync(cancellationToken);
        var apiUrl = $"{options.BaseUrl.TrimEnd('/')}/OmniChannel/Contract/Connect/Car/Certificate/Insert";
        var request = _vniRequestMapper.ToCarVcxRequest(
            context.Policy, context.CurrentVersion, product, context.FirstMotor);
        var requestJson = JsonSerializer.Serialize(request, LogJsonOptions);

        var sw = Stopwatch.StartNew();
        try
        {
            var vniResult = await _vniApiClient.IssueCarVcxPolicyAsync(request, cancellationToken);
            sw.Stop();

            var responseJson = JsonSerializer.Serialize(
                new { vniResult.Code, vniResult.Message, vniResult.Data, vniResult.Total }, LogJsonOptions);

            await LogMessageAsync(
                context.PolicyId, apiUrl, "POST", requestJson, responseJson,
                vniResult.IsSuccess ? 200 : 400, vniResult.IsSuccess,
                vniResult.IsSuccess ? null : $"{vniResult.Code} - {vniResult.Message}",
                sw.ElapsedMilliseconds, cancellationToken);

            if (!vniResult.IsSuccess)
            {
                var msg = $"VNI Car VCX issue failed: {vniResult.Code} - {vniResult.Message}";
                _logger.LogWarning(msg);
                return new PartnerIntegrationResult
                {
                    ProductCode = "VCX", IsSuccess = false, ErrorMessage = msg
                };
            }

            return new PartnerIntegrationResult { ProductCode = "VCX", IsSuccess = true };
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex, "VNI Car VCX issue API call failed for policy {PolicyId}", context.PolicyId);

            await LogMessageAsync(
                context.PolicyId, apiUrl, "POST", requestJson, null,
                null, false, ex.Message, sw.ElapsedMilliseconds, cancellationToken);

            return new PartnerIntegrationResult
            {
                ProductCode = "VCX", IsSuccess = false, ErrorMessage = $"VCX: {ex.Message}"
            };
        }
    }

    private async Task LogMessageAsync(
        Guid policyId, string apiUrl, string httpMethod,
        string? requestBody, string? responseBody,
        int? httpStatusCode, bool isSuccess, string? errorMessage,
        long? durationMs, CancellationToken cancellationToken)
    {
        try
        {
            var entry = new ResPartnerMessageLogging(
                Guid.NewGuid(),
                PartnerCodeVni,
                policyId,
                apiUrl,
                httpMethod,
                requestBody,
                responseBody,
                httpStatusCode,
                isSuccess,
                errorMessage,
                durationMs);

            await _loggingService.LogAsync(entry, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log partner message for policy {PolicyId}", policyId);
        }
    }
}
