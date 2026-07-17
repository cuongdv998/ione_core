using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using iOne.AccountPaymentRequests;
using iOne.AdminConfigs;
using iOne.Payment.VnPay;
using iOne.Policies;
using iOne.Policy.Policies;
using iOne.ProLineOfBusinesses;
using iOne.ResPartners;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp;
using PolicyEntity = iOne.Policies.Policy;

namespace iOne.Payment.Payments;

public class PaymentAppService : ApplicationService, IPaymentAppService
{
    private const string MotorbikeLobAdminConfigCode = "DEFAULT_LOB_MOTORBIKE";
    private const string MotorbikeLobFallbackCode = "MOTOR";
    private const string InsurerCodePti = "PTI";
    private const string VnPayProvider = "VNPAY";
    private const string UnpaidPaymentStatus = "new";

    private readonly IRepository<AccountPaymentRequest, Guid> _accountPaymentRequestRepository;
    private readonly IRepository<PolicyEntity, Guid> _policyRepository;
    private readonly IRepository<PolicyVersion, Guid> _policyVersionRepository;
    private readonly IRepository<PolicyAmount, Guid> _policyAmountRepository;
    private readonly IRepository<ProLineOfBusiness, Guid> _lobRepository;
    private readonly IRepository<AdminConfig, Guid> _adminConfigRepository;
    private readonly IRepository<ResPartner, Guid> _resPartnerRepository;
    private readonly IVnPayAppService _vnPayAppService;
    private readonly IPolicyPartnerAppService _policyPartnerAppService;

    public PaymentAppService(
        IRepository<AccountPaymentRequest, Guid> accountPaymentRequestRepository,
        IRepository<PolicyEntity, Guid> policyRepository,
        IRepository<PolicyVersion, Guid> policyVersionRepository,
        IRepository<PolicyAmount, Guid> policyAmountRepository,
        IRepository<ProLineOfBusiness, Guid> lobRepository,
        IRepository<AdminConfig, Guid> adminConfigRepository,
        IRepository<ResPartner, Guid> resPartnerRepository,
        IVnPayAppService vnPayAppService,
        IPolicyPartnerAppService policyPartnerAppService)
    {
        _accountPaymentRequestRepository = accountPaymentRequestRepository;
        _policyRepository = policyRepository;
        _policyVersionRepository = policyVersionRepository;
        _policyAmountRepository = policyAmountRepository;
        _lobRepository = lobRepository;
        _adminConfigRepository = adminConfigRepository;
        _resPartnerRepository = resPartnerRepository;
        _vnPayAppService = vnPayAppService;
        _policyPartnerAppService = policyPartnerAppService;
    }

    public async Task<MotorbikePaymentInquiryResultDto> ProcessPaymentInquiryAsync()
    {
        Logger.LogInformation("Motorbike payment-inquiry batch started.");

        var result = new MotorbikePaymentInquiryResultDto();
        var motorbikeLobId = await ResolveMotorbikeLobIdAsync();
        var draftStatus = PolicyStatus.Draft.ToString().ToLowerInvariant();

        var policyQuery = await _policyRepository.GetQueryableAsync();
        var versionQuery = await _policyVersionRepository.GetQueryableAsync();
        var amountQuery = await _policyAmountRepository.GetQueryableAsync();
        var partnerQuery = await _resPartnerRepository.GetQueryableAsync();

        var candidates = await AsyncExecuter.ToListAsync(
            from policy in policyQuery
            join version in versionQuery on policy.LastVersionId equals version.Id
            join partner in partnerQuery on policy.PartnerId equals partner.Id
            join amount in amountQuery on version.Id equals amount.PolicyVersionId
            where policy.LobId == motorbikeLobId
                  && !policy.IsDeleted
                  && !version.IsDeleted
                  && version.Status == draftStatus
                  && !amount.IsDeleted
                  && amount.PaymentStatus.ToLower() == UnpaidPaymentStatus
            orderby policy.CreationTime descending
            select new MotorbikePaymentInquiryCandidate
            {
                PolicyId = policy.Id,
                InsurerCode = partner.Code
            });

        var distinctCandidates = candidates
            .GroupBy(x => x.PolicyId)
            .Select(g => g.First())
            .ToList();

        result.CandidateCount = distinctCandidates.Count;
        result.CandidatePolicyIds = distinctCandidates.Select(x => x.PolicyId).ToList();

        Logger.LogInformation(
            "Motorbike payment-inquiry: found {CandidateCount} candidate policies (draft, unpaid, motor LOB). PolicyIds={PolicyIds}",
            result.CandidateCount,
            string.Join(", ", result.CandidatePolicyIds));

        foreach (var candidate in distinctCandidates)
        {
            var insurerCode = candidate.InsurerCode?.Trim() ?? string.Empty;

            if (!string.Equals(insurerCode, InsurerCodePti, StringComparison.OrdinalIgnoreCase))
            {
                result.SkippedNonPtiCount++;
                Logger.LogInformation(
                    "Motorbike payment-inquiry: skip policy {PolicyId}, insurer code {InsurerCode} is not PTI",
                    candidate.PolicyId,
                    string.IsNullOrEmpty(insurerCode) ? "(empty)" : insurerCode);
                continue;
            }

            result.PtiInquiryCount++;
            result.InquiredPolicyIds.Add(candidate.PolicyId);

            Logger.LogInformation(
                "Motorbike payment-inquiry: syncing certificate (partner-policy-inquiry) for policy {PolicyId}, insurer {InsurerCode}",
                candidate.PolicyId,
                insurerCode);

            try
            {
                await _policyPartnerAppService.PartnerPolicyInquiryAsync(candidate.PolicyId.ToString());
                result.SuccessCount++;
                Logger.LogInformation(
                    "Motorbike payment-inquiry: partner-policy-inquiry succeeded for policy {PolicyId}",
                    candidate.PolicyId);
            }
            catch (Exception ex)
            {
                result.FailedCount++;
                Logger.LogWarning(
                    ex,
                    "Motorbike payment-inquiry: partner-policy-inquiry failed for policy {PolicyId}",
                    candidate.PolicyId);
            }
        }

        Logger.LogInformation(
            "Motorbike payment-inquiry batch finished. Candidates={CandidateCount}, PtiInquired={PtiInquiryCount}, Success={SuccessCount}, Failed={FailedCount}, SkippedNonPti={SkippedNonPtiCount}, InquiredPolicyIds={InquiredPolicyIds}",
            result.CandidateCount,
            result.PtiInquiryCount,
            result.SuccessCount,
            result.FailedCount,
            result.SkippedNonPtiCount,
            string.Join(", ", result.InquiredPolicyIds));

        return result;
    }

    public async Task ProcessPaymentInquiryByIdAsync(Guid paymentId)
    {
        var paymentQuery = await _accountPaymentRequestRepository.GetQueryableAsync();
        var policyQuery = await _policyRepository.GetQueryableAsync();
        var lobQuery = await _lobRepository.GetQueryableAsync();

        var payment = await (
            from p in paymentQuery
            join policy in policyQuery on p.PolicyId equals policy.Id
            join lob in lobQuery on policy.LobId equals lob.Id
            where p.Id == paymentId
                  && p.Status == AccountPaymentRequestStatus.Draft
                  && p.PolicyId != null
                  && p.PaymentProvider != null
                  && p.PaymentProvider.Trim() != string.Empty
                  && lob.Code != null
                  && lob.Code.ToUpper() == MotorbikeLobFallbackCode
            select new PaymentInquiryItemDto
            {
                PaymentId = p.Id,
                PolicyId = p.PolicyId,
                Amount = p.Amount,
                IssueDate = p.IssueDate,
                DueDate = p.DueDate,
                CreationTime = p.CreationTime,
                Status = p.Status,
                PaymentProvider = p.PaymentProvider,
                TransRef = p.TransRef
            }
        ).FirstOrDefaultAsync();

        if (payment == null)
        {
            throw new UserFriendlyException("Payment is not eligible for inquiry.");
        }

        await ProcessSinglePaymentInquiryAsync(payment);
    }

    private async Task<Guid> ResolveMotorbikeLobIdAsync()
    {
        var motorbikeLobConfig = await AsyncExecuter.FirstOrDefaultAsync(
            (await _adminConfigRepository.GetQueryableAsync()).Where(x =>
                x.Code == MotorbikeLobAdminConfigCode &&
                x.Status == AdminConfigStatus.Active));

        var motorbikeLobCode = motorbikeLobConfig?.Value?.Trim();
        if (string.IsNullOrWhiteSpace(motorbikeLobCode))
        {
            motorbikeLobCode = MotorbikeLobFallbackCode;
            Logger.LogWarning(
                "Motorbike payment-inquiry: admin config {AdminConfigCode} missing; fallback LOB code {LobCode}",
                MotorbikeLobAdminConfigCode,
                motorbikeLobCode);
        }

        var lobQuery = await _lobRepository.GetQueryableAsync();
        var motorbikeLob = await AsyncExecuter.FirstOrDefaultAsync(
            lobQuery.Where(x => x.Code == motorbikeLobCode));

        if (motorbikeLob == null)
        {
            throw new UserFriendlyException($"Motorbike LOB not found (code: {motorbikeLobCode}).");
        }

        return motorbikeLob.Id;
    }

    private async Task ProcessSinglePaymentInquiryAsync(PaymentInquiryItemDto payment)
    {
        var provider = payment.PaymentProvider?.Trim();
        if (!string.Equals(provider, VnPayProvider, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(payment.TransRef))
        {
            Logger.LogWarning("Skip VNPAY inquiry for payment {PaymentId} because TransRef is empty.", payment.PaymentId);
            return;
        }

        var inquiryInput = new VnPayQueryDrRequestDto
        {
            TxnRef = payment.TransRef.Trim(),
            OrderInfo = string.IsNullOrWhiteSpace(payment.TransRef)
                ? $"PAYMENT-{payment.PaymentId:N}"
                : payment.TransRef.Trim(),
            TransactionDate = payment.CreationTime.ToString("yyyyMMddHHmmss"),
            IpAddress = "127.0.0.1"
        };

        try
        {
            await _vnPayAppService.QueryTransactionAsync(inquiryInput);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "VNPAY inquiry failed for payment {PaymentId}.", payment.PaymentId);
        }
    }

    private sealed class MotorbikePaymentInquiryCandidate
    {
        public Guid PolicyId { get; init; }

        public string? InsurerCode { get; init; }
    }
}
