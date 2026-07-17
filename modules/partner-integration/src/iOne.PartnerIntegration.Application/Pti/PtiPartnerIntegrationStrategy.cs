using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using iOne.PartnerIntegration;
using iOne.PartnerIntegration.Logging;
using iOne.PartnerIntegration.Pti.Models;
using iOne.ResPartnerMessageLoggings;
using Microsoft.Extensions.Logging;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace iOne.PartnerIntegration.Pti;

public class PtiPartnerIntegrationStrategy : IPartnerIntegrationStrategy, ITransientDependency
{
    private const string PartnerCodePti = "PTI";

    private readonly IPtiApiClient _apiClient;
    private readonly PtiRequestMapper _requestMapper;
    private readonly IPtiConfigProvider _configProvider;
    private readonly IPartnerMessageLoggingService _loggingService;
    private readonly ILogger<PtiPartnerIntegrationStrategy> _logger;

    private static readonly JsonSerializerOptions LogJsonOptions = new() { WriteIndented = false };

    public PtiPartnerIntegrationStrategy(
        IPtiApiClient apiClient,
        PtiRequestMapper requestMapper,
        IPtiConfigProvider configProvider,
        IPartnerMessageLoggingService loggingService,
        ILogger<PtiPartnerIntegrationStrategy> logger)
    {
        _apiClient = apiClient;
        _requestMapper = requestMapper;
        _configProvider = configProvider;
        _loggingService = loggingService;
        _logger = logger;
    }

    public bool CanHandle(string partnerCode)
        => string.Equals(partnerCode, PartnerCodePti, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Not used for motorbike flow — delegates to <see cref="IssueMotorPolicyAsync"/>.
    /// </summary>
    public Task<List<PartnerIntegrationResult>> IssuePolicyAsync(
        PartnerIssuePolicyContext context, CancellationToken cancellationToken = default)
        => throw new NotSupportedException("PTI strategy uses IssueMotorPolicyAsync for motorbike policies.");

    public async Task<MotorPolicyIssueResult> IssueMotorPolicyAsync(
        PartnerIssuePolicyContext context, CancellationToken cancellationToken = default)
    {
        var options = await _configProvider.GetOptionsAsync(cancellationToken);
        var apiUrl = $"{options.BaseUrl.TrimEnd('/')}/vehicle/create";

        var request = await _requestMapper.ToRequestAsync(
            context.Policy,
            context.CurrentVersion,
            context.Products,
            context.FirstMotor,
            options.ChannelCode,
            context.BillInfo,
            context.OwnerInfo,
            cancellationToken);

        var requestJson = JsonSerializer.Serialize(request, LogJsonOptions);
        _logger.LogInformation("PTI Payload gửi sang pti: {Payload}", requestJson);

        var sw = Stopwatch.StartNew();
        PtiCreateVehicleResponse? response = null;
        string? responseJson = null;
        int? httpStatusForLog = null;
        Exception? callException = null;

        try
        {
            response = await _apiClient.IssueVehicleAsync(request, cancellationToken);
            sw.Stop();
            responseJson = JsonSerializer.Serialize(response, LogJsonOptions);
            httpStatusForLog = 200;
        }
        catch (PtiPartnerHttpException ex)
        {
            sw.Stop();
            callException = ex;
            responseJson = ex.RawResponseBody;
            httpStatusForLog = ex.HttpStatusCode;
            _logger.LogError(ex, "PTI vehicle/create failed for policy {PolicyId}", context.PolicyId);
        }
        catch (Exception ex)
        {
            sw.Stop();
            callException = ex;
            _logger.LogError(ex, "PTI vehicle/create failed for policy {PolicyId}", context.PolicyId);
        }

        await LogPartnerMessageAsync(
            context.PolicyId,
            apiUrl,
            "POST",
            requestJson,
            responseJson,
            httpStatusForLog,
            callException == null,
            callException?.Message,
            sw.ElapsedMilliseconds,
            cancellationToken);

        if (callException != null)
        {
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(callException).Throw();
        }

        var payment = response!.PaymentInfoDetail;

        return new MotorPolicyIssueResult
        {
            TransId = request.TransId,
            TaskId = response.TaskId.ToString(),
            ContractNumber = response.ContractNumber,
            PartnerResponse = new Dictionary<string, object?>
            {
                ["qrCode"]         = (payment?.QrCode?.Count >= 2) ? payment.QrCode[1] : null,
                ["paymentExpired"] = payment?.PaymentExpired,
                ["paymentAmount"]  = payment?.PaymentAmount
            }
        };
    }

    public async Task<PartnerPolicyInquiryResult> InquiryPolicyAsync(
        PartnerPolicyInquiryContext context, CancellationToken cancellationToken = default)
    {
        var transId = context.Policy.Id.ToString();
        if (string.IsNullOrWhiteSpace(transId))
            throw new BusinessException("Policy:Partner:PtiInsurerPolicyNoMissing")
                .WithData("PolicyId", context.PolicyId);

        var sw = Stopwatch.StartNew();
        string? apiUrl = null;
        string? responseJson = null;
        int? httpStatus = null;
        Exception? callException = null;

        try
        {
            var (url, rawJson, successStatus) = await _apiClient.LookupVehicleInfoAsync(transId, cancellationToken);
            sw.Stop();
            apiUrl = url;
            responseJson = rawJson;
            httpStatus = successStatus;
        }
        catch (PtiPartnerHttpException ex)
        {
            sw.Stop();
            callException = ex;
            if (!string.IsNullOrEmpty(ex.RequestUrl))
                apiUrl = ex.RequestUrl;
            responseJson = ex.RawResponseBody;
            httpStatus = ex.HttpStatusCode;
            _logger.LogError(ex, "PTI vehicle/lookup-info failed for policy {PolicyId}, transId={TransId}",
                context.PolicyId, transId);
        }
        catch (Exception ex)
        {
            sw.Stop();
            callException = ex;
            _logger.LogError(ex, "PTI vehicle/lookup-info failed for policy {PolicyId}, transId={TransId}",
                context.PolicyId, transId);
        }

        await LogPartnerMessageAsync(
            context.PolicyId,
            apiUrl ?? string.Empty,
            "GET",
            null,
            responseJson,
            httpStatus,
            callException == null,
            callException?.Message,
            sw.ElapsedMilliseconds,
            cancellationToken);

        if (callException != null)
        {
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(callException).Throw();
        }

        var partnerResponse = ParseJsonToDictionary(responseJson);

        var certificateSnapshots = new List<PtiCertificateInfoSnapshot>();
        PtiVehicleLookupInfoParser.TryExtractLookupInfo(
            responseJson,
            out var suggestedInsurerPolicyNo,
            out var suggestedInsurerContractCode,
            certificateSnapshots);

        return new PartnerPolicyInquiryResult
        {
            PartnerResponse = partnerResponse,
            SuggestedInsurerPolicyNo = suggestedInsurerPolicyNo,
            SuggestedInsurerContractCode = suggestedInsurerContractCode,
            SuggestedCertificateInfos = certificateSnapshots
        };
    }

    /// <summary>
    /// Recursively converts a <see cref="JsonElement"/> to a plain .NET object suitable
    /// for serialization as a flexible <c>Dictionary&lt;string, object?&gt;</c> response.
    /// </summary>
    private static Dictionary<string, object?> ParseJsonToDictionary(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new Dictionary<string, object?>();

        try
        {
            var doc = JsonDocument.Parse(json);
            return JsonElementToObject(doc.RootElement) as Dictionary<string, object?>
                   ?? new Dictionary<string, object?> { ["raw"] = json };
        }
        catch
        {
            return new Dictionary<string, object?> { ["raw"] = json };
        }
    }

    private static object? JsonElementToObject(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Object => JsonObjectToDictionary(element),
            JsonValueKind.Array  => JsonArrayToList(element),
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.TryGetInt64(out var l) ? (object?)l
                                  : element.TryGetDouble(out var d) ? d
                                  : element.GetRawText(),
            JsonValueKind.True  => true,
            JsonValueKind.False => false,
            JsonValueKind.Null  => null,
            _                   => element.GetRawText()
        };
    }

    private static Dictionary<string, object?> JsonObjectToDictionary(JsonElement obj)
    {
        var dict = new Dictionary<string, object?>();
        foreach (var prop in obj.EnumerateObject())
            dict[prop.Name] = JsonElementToObject(prop.Value);
        return dict;
    }

    private static List<object?> JsonArrayToList(JsonElement array)
    {
        var list = new List<object?>();
        foreach (var item in array.EnumerateArray())
            list.Add(JsonElementToObject(item));
        return list;
    }

    private async Task LogPartnerMessageAsync(
        Guid policyId, string apiUrl, string httpMethod,
        string? requestBody, string? responseBody,
        int? httpStatusCode, bool isSuccess, string? errorMessage,
        long? durationMs, CancellationToken cancellationToken)
    {
        try
        {
            var entry = new ResPartnerMessageLogging(
                Guid.NewGuid(),
                PartnerCodePti,
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
            _logger.LogError(ex, "Failed to log PTI partner message for policy {PolicyId}", policyId);
        }
    }
}
