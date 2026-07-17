using System;
using System.Linq;
using System.Threading.Tasks;
using iOne.AccountPaymentRequests;
using iOne.Policies;
using iOne.Policy.Policies;
using iOne.Master.SystemEventNotifies;
using Microsoft.Extensions.Logging;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using iOne.WebviewAuth;
using Microsoft.EntityFrameworkCore;

namespace iOne.Partner.PaymentCallbacks;

public class PaymentCallbackAppService : ApplicationService, IPaymentCallbackAppService
{
    private const string PaymentResultSuccess = "SUCCESS";
    private const string PaymentResultFail = "FAIL";
    private const string PtiPaymentProvider = "PTI";

    private readonly IPolicyRepository _policyRepository;
    private readonly IPolicyAppService _policyAppService;
    private readonly IRepository<AccountPaymentRequest, Guid> _accountPaymentRequestRepository;
    private readonly IRepository<PolicyCertificate, Guid> _policyCertificateRepository;
    private readonly INotificationHubService _notificationHubService;
    private readonly IPartnerWebviewTokenValidator _partnerWebviewTokenValidator;

    public PaymentCallbackAppService(
        IPolicyRepository policyRepository,
        IPolicyAppService policyAppService,
        IRepository<AccountPaymentRequest, Guid> accountPaymentRequestRepository,
        IRepository<PolicyCertificate, Guid> policyCertificateRepository,
        INotificationHubService notificationHubService,
        IPartnerWebviewTokenValidator partnerWebviewTokenValidator)
    {
        _policyRepository = policyRepository;
        _policyAppService = policyAppService;
        _accountPaymentRequestRepository = accountPaymentRequestRepository;
        _policyCertificateRepository = policyCertificateRepository;
        _notificationHubService = notificationHubService;
        _partnerWebviewTokenValidator = partnerWebviewTokenValidator;
    }

    public async Task<PaymentCallbackResponseDto> ProcessCallbackAsync(PaymentCallbackRequestDto input)
    {
        Logger.LogInformation("Payment callback received for TransId: {TransId}, Result: {Result}", input.TransId, input.PaymentResult);

        try
        {
            var paymentResult = input.PaymentResult.Trim();
            if (string.Equals(paymentResult, PaymentResultSuccess, StringComparison.OrdinalIgnoreCase))
            {
                return await ProcessPaymentSuccessAsync(input);
            }

            if (string.Equals(paymentResult, PaymentResultFail, StringComparison.OrdinalIgnoreCase))
            {
                return await ProcessPaymentFailureAsync(input);
            }

            Logger.LogWarning("Unsupported PaymentResult: {Result} for TransId: {TransId}", paymentResult, input.TransId);
            return new PaymentCallbackResponseDto
            {
                Code = "4000",
                Message = "Kết quả thanh toán không hợp lệ."
            };
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error processing callback for TransId: {TransId}", input.TransId);
            return new PaymentCallbackResponseDto
            {
                Code = "5000",
                Message = "Lỗi hệ thống."
            };
        }
    }

    private async Task<PaymentCallbackResponseDto> ProcessPaymentSuccessAsync(PaymentCallbackRequestDto input)
    {
        Logger.LogInformation("Body from PTI: {Input}", System.Text.Json.JsonSerializer.Serialize(input));
        var transId = input.TransId.Trim();

        var policy = await FindPolicyByTransIdAsync(transId);

        if (policy == null)
        {
            Logger.LogWarning("No policy found with Id matching TransId: {TransId}", transId);
        }
        else
        {
            await UpdateAccountPaymentRequestByPolicyAsync(policy.Id, transId, AccountPaymentRequestStatus.Approved);

            Logger.LogInformation(
                "Marking policy_amount as paid for policy {PolicyId} after PTI payment success (TransId: {TransId})",
                policy.Id,
                transId);
            await _policyAppService.MarkPolicyAmountsAsPaidAsync(policy.Id);

            Logger.LogInformation("Submitting policy {PolicyId} for approval (TransId: {TransId})", policy.Id, transId);
            await _policyAppService.SubmitForApprovalAsync(policy.Id, new SubmitForApprovalInput());
            Logger.LogInformation("Policy {PolicyId} successfully submitted for approval", policy.Id);


            if (input.Cert != null && !string.IsNullOrWhiteSpace(input.Cert.CertificationLink))
            {
                Logger.LogInformation("Saving certificate info for Policy {PolicyId}, TransId: {TransId}", policy.Id, transId);
                await SavePolicyCertificateAsync(policy, input.Cert);

                Logger.LogInformation("Sending certificate URL to Amio for TransId: {TransId}", transId);
                try
                {
                    var lastVersion = policy.PolicyVersions.FirstOrDefault(v => v.Id == policy.LastVersionId);
                    var riskObject = lastVersion?.PolicyRiskObjects.FirstOrDefault();
                    var riskMotor = riskObject?.PolicyRiskMotors.FirstOrDefault();

                    await _partnerWebviewTokenValidator.SendOrderResultAsync(new SendOrderResultInputDto
                    {
                        TransId = input.TransId,
                        OrderId = policy.Id.ToString(),
                        Product = lastVersion?.PolicyProducts.FirstOrDefault()?.Product?.Code,
                        Amount = input.PaymentAmount,
                        CustomerCode = policy.Contract?.Customer?.RefCode,
                        CertificateUrl = input.Cert.CertificationLink,
                        ContractNumber = input.Cert.CertificationNo,
                        VehicleOwner = riskObject?.RepName,
                        LicensePlate = riskMotor?.CarPlate,
                        EffectivePeriod = lastVersion != null
                            ? $"{lastVersion.EffectDate:dd/MM/yyyy} - {lastVersion.ExpireDate:dd/MM/yyyy}"
                            : null
                    });
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Failed to send order result to Amio for TransId: {TransId}", transId);
                }
            }
            await _notificationHubService.SendCustomEventToPolicyAsync(
                transId,
                "CloseQrCommand",
                new { success = true, transId, message = "Thanh toán thành công" });
        }

        return new PaymentCallbackResponseDto
        {
            Code = "0000",
            Message = "Thành công"
        };
    }

    private async Task<PaymentCallbackResponseDto> ProcessPaymentFailureAsync(PaymentCallbackRequestDto input)
    {
        var transId = input.TransId.Trim();

        var policy = await FindPolicyByTransIdAsync(transId);

        if (policy == null)
        {
            Logger.LogWarning("No policy found with Id matching TransId: {TransId} (payment fail)", transId);
        }
        else
        {
            await UpdateAccountPaymentRequestByPolicyAsync(policy.Id, transId, AccountPaymentRequestStatus.Rejected);
            Logger.LogInformation("Payment failed callback recorded for policy {PolicyId}, TransId: {TransId}", policy.Id, transId);

            await _notificationHubService.SendCustomEventToPolicyAsync(
                policy.Id.ToString(),
                "CloseQrCommand",
                new { success = false, transId, message = "Thanh toán thất bại" });
        }

        return new PaymentCallbackResponseDto
        {
            Code = "0000",
            Message = "Thành công"
        };
    }

    private async Task<iOne.Policies.Policy?> FindPolicyByTransIdAsync(string transId)
    {
        if (!Guid.TryParse(transId, out var policyId))
        {
            return null;
        }

        var policyQuery = await _policyRepository.GetQueryableAsync();
        return await AsyncExecuter.FirstOrDefaultAsync(
            policyQuery
                .Include(p => p.Contract)
                    .ThenInclude(c => c!.Customer)
                .Include(p => p.PolicyVersions)
                    .ThenInclude(v => v.PolicyProducts)
                        .ThenInclude(pp => pp.Product)
                .Include(p => p.PolicyVersions)
                    .ThenInclude(v => v.PolicyRiskObjects)
                        .ThenInclude(ro => ro.PolicyRiskMotors)
                .Where(p => p.Id == policyId));
    }

    /// <summary>
    /// Cập nhật bản ghi thanh toán gần nhất theo policy (transref, payment_provider=PTI, status).
    /// </summary>
    private async Task UpdateAccountPaymentRequestByPolicyAsync(
        Guid policyId,
        string transRef,
        AccountPaymentRequestStatus status)
    {
        var query = await _accountPaymentRequestRepository.GetQueryableAsync();
        var paymentRequest = await AsyncExecuter.FirstOrDefaultAsync(
            query
                .Where(p => p.PolicyId == policyId)
                .OrderByDescending(p => p.CreationTime));

        if (paymentRequest == null)
        {
            Logger.LogWarning("No account_payment_request found for PolicyId: {PolicyId}", policyId);
            return;
        }

        paymentRequest.UpdateTransRef(transRef);
        paymentRequest.UpdatePaymentProvider(PtiPaymentProvider);
        paymentRequest.UpdateStatus(status);
        await _accountPaymentRequestRepository.UpdateAsync(paymentRequest, autoSave: true);
    }

    private async Task SavePolicyCertificateAsync(iOne.Policies.Policy policy, CertInfo certInfo)
    {
        var query = await _policyCertificateRepository.GetQueryableAsync();
        var existing = await AsyncExecuter.FirstOrDefaultAsync(
            query.Where(x => x.PolicyId == policy.Id && x.PolicyVersionId == policy.LastVersionId));

        if (existing != null)
        {
            existing.UpdateCertificateNo(certInfo.CertificationNo);
            existing.UpdateUrl(certInfo.CertificationLink);
            await _policyCertificateRepository.UpdateAsync(existing, autoSave: true);
        }
        else
        {
            await _policyCertificateRepository.InsertAsync(new PolicyCertificate(
                GuidGenerator.Create(),
                policy.Id,
                policy.LastVersionId,
                certInfo.CertificationNo,
                certInfo.CertificationLink
            ), autoSave: true);
        }
    }
}
