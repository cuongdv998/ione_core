using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using iOne.AccountPaymentRequests;
using iOne.HrEmployees;
using iOne.PartnerIntegration;
using iOne.Policies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using PolicyEntity = iOne.Policies.Policy;
using PolicyVersionEntity = iOne.Policies.PolicyVersion;

namespace iOne.Policy.Policies;

public class PolicyPartnerAppService : ApplicationService, IPolicyPartnerAppService
{
    private const string PartnerCodePti = "PTI";

    private const int PolicyCertificateCertificateNoMaxLength = 50;
    private const int PolicyCertificateUrlMaxLength = 250;

    private readonly IRepository<PolicyEntity, Guid> _policyRepository;
    private readonly IRepository<PolicyVersionEntity, Guid> _policyVersionRepository;
    private readonly IPartnerIntegrationService _partnerIntegrationService;
    private readonly IRepository<AccountPaymentRequest, Guid> _accountPaymentRequestRepository;
    private readonly IRepository<HrEmployee, Guid> _hrEmployeeRepository;
    private readonly IPolicyAppService _policyAppService;
    private readonly IPolicyCertificateRepository _policyCertificateRepository;
    private readonly IGuidGenerator _guidGenerator;

    public PolicyPartnerAppService(
        IRepository<PolicyEntity, Guid> policyRepository,
        IRepository<PolicyVersionEntity, Guid> policyVersionRepository,
        IPartnerIntegrationService partnerIntegrationService,
        IRepository<AccountPaymentRequest, Guid> accountPaymentRequestRepository,
        IRepository<HrEmployee, Guid> hrEmployeeRepository,
        IPolicyAppService policyAppService,
        IPolicyCertificateRepository policyCertificateRepository,
        IGuidGenerator guidGenerator)
    {
        _policyRepository = policyRepository;
        _policyVersionRepository = policyVersionRepository;
        _partnerIntegrationService = partnerIntegrationService;
        _accountPaymentRequestRepository = accountPaymentRequestRepository;
        _hrEmployeeRepository = hrEmployeeRepository;
        _policyAppService = policyAppService;
        _policyCertificateRepository = policyCertificateRepository;
        _guidGenerator = guidGenerator;
    }

    public async Task<PartnerPolicyInquiryResultDto> PartnerPolicyInquiryAsync(string policyId)
    {
        if (string.IsNullOrWhiteSpace(policyId) || !Guid.TryParse(policyId.Trim(), out var id))
            throw new BusinessException("Policy:PartnerPolicyInquiry:InvalidPolicyId")
                .WithData("PolicyId", policyId);

        var query = await _policyRepository.GetQueryableAsync();
        var policy = await query
            .Include(p => p.Partner)
            .Include(p => p.Contract)
            .Include(p => p.PolicyVersions)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (policy == null)
            throw new BusinessException("Policy:PartnerPolicyInquiry:PolicyNotFound")
                .WithData("PolicyId", policyId);

        var result = await _partnerIntegrationService.InquiryPolicyAsync(policy);

        var partnerCode = policy.Partner?.Code?.Trim();
        if (string.Equals(partnerCode, PartnerCodePti, StringComparison.OrdinalIgnoreCase))
        {
            await ApplyPtiInquirySuccessSideEffectsAsync(policy, result);
        }

        return new PartnerPolicyInquiryResultDto
        {
            PartnerResponse = result.PartnerResponse
        };
    }

    private async Task ApplyPtiInquirySuccessSideEffectsAsync(
        PolicyEntity policy,
        PartnerPolicyInquiryResult inquiryResult)
    {
        var changedPolicy = false;

        if (!string.IsNullOrWhiteSpace(inquiryResult.SuggestedInsurerPolicyNo))
        {
            policy.UpdateInsurerPolicyNo(inquiryResult.SuggestedInsurerPolicyNo);
            changedPolicy = true;
        }

        if (policy.Contract != null && !string.IsNullOrWhiteSpace(inquiryResult.SuggestedInsurerContractCode))
        {
            policy.Contract.UpdateInsurerContractCode(inquiryResult.SuggestedInsurerContractCode);
            changedPolicy = true;
        }

        if (changedPolicy)
        {
            await _policyRepository.UpdateAsync(policy);
        }

        await ApplyPtiCertificateSyncFromInquiryAsync(policy, inquiryResult.SuggestedCertificateInfos);

        if (!InquiryResultHasCertificateInfos(inquiryResult))
        {
            return;
        }

        await TryApproveLatestPolicyPaymentAsync(policy.Id);
        await _policyAppService.MarkPolicyAmountsAsPaidAsync(policy.Id);
        await TrySubmitDraftPolicyForApprovalAsync(policy);
    }

    /// <summary>
    /// PTI lookup trả <c>certificateInfos</c> có ít nhất một mục có số GCN hoặc URL — coi là đã có giấy chứng nhận.
    /// </summary>
    private static bool InquiryResultHasCertificateInfos(PartnerPolicyInquiryResult inquiryResult)
    {
        var snapshots = inquiryResult.SuggestedCertificateInfos;
        if (snapshots == null || snapshots.Count == 0)
        {
            return false;
        }

        return snapshots.Exists(s =>
            !string.IsNullOrWhiteSpace(s.CertificateCode) || !string.IsNullOrWhiteSpace(s.Url));
    }

    private async Task ApplyPtiCertificateSyncFromInquiryAsync(
        PolicyEntity policy,
        IList<PtiCertificateInfoSnapshot>? snapshots)
    {
        if (snapshots == null || snapshots.Count == 0)
        {
            return;
        }

        if (policy.LastVersionId == Guid.Empty)
        {
            return;
        }

        if (policy.PolicyVersions == null ||
            policy.PolicyVersions.All(v => v.Id != policy.LastVersionId))
        {
            Logger.LogWarning(
                "PTI inquiry: policy {PolicyId} LastVersionId {VersionId} not in loaded versions; skipping certificate sync.",
                policy.Id, policy.LastVersionId);
            return;
        }

        var queryable = await _policyCertificateRepository.GetQueryableAsync();
        var existing = await AsyncExecuter.ToListAsync(
            queryable
                .Where(c => c.PolicyId == policy.Id &&
                            c.PolicyVersionId == policy.LastVersionId &&
                            !c.IsDeleted)
                .OrderBy(c => c.CreationTime)
                .ThenBy(c => c.Id));

        for (var i = 0; i < snapshots.Count; i++)
        {
            var snap = snapshots[i];
            if (i < existing.Count)
            {
                var cert = existing[i];
                var changed = false;
                if (!string.IsNullOrWhiteSpace(snap.CertificateCode))
                {
                    cert.UpdateCertificateNo(TruncateCertificateNo(snap.CertificateCode.Trim(), policy.Id));
                    changed = true;
                }
                if (!string.IsNullOrWhiteSpace(snap.Url))
                {
                    var url = TruncateUrlForCertificate(snap.Url.Trim(), policy.Id);
                    cert.UpdateUrl(url);
                    changed = true;
                }
                if (changed)
                {
                    await _policyCertificateRepository.UpdateAsync(cert);
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(snap.CertificateCode) && string.IsNullOrWhiteSpace(snap.Url))
                {
                    continue;
                }

                var code = string.IsNullOrWhiteSpace(snap.CertificateCode)
                    ? null
                    : TruncateCertificateNo(snap.CertificateCode.Trim(), policy.Id);
                string? url = null;
                if (!string.IsNullOrWhiteSpace(snap.Url))
                {
                    url = TruncateUrlForCertificate(snap.Url.Trim(), policy.Id);
                }

                var newCert = new PolicyCertificate(
                    _guidGenerator.Create(),
                    policy.Id,
                    policy.LastVersionId,
                    certificateNo: code,
                    url: url);
                await _policyCertificateRepository.InsertAsync(newCert);
            }
        }
    }

    private string? TruncateCertificateNo(string value, Guid policyId)
    {
        if (value.Length <= PolicyCertificateCertificateNoMaxLength)
        {
            return value;
        }

        Logger.LogWarning(
            "PTI inquiry: certificateCode truncated for policy {PolicyId}: length {Length} exceeds {Max}.",
            policyId, value.Length, PolicyCertificateCertificateNoMaxLength);
        return value[..PolicyCertificateCertificateNoMaxLength];
    }

    private string TruncateUrlForCertificate(string value, Guid policyId)
    {
        if (value.Length <= PolicyCertificateUrlMaxLength)
        {
            return value;
        }

        Logger.LogWarning(
            "PTI inquiry: certificate url truncated for policy {PolicyId}: length {Length} exceeds {Max}.",
            policyId, value.Length, PolicyCertificateUrlMaxLength);
        return value[..PolicyCertificateUrlMaxLength];
    }

    private async Task TryApproveLatestPolicyPaymentAsync(Guid policyId)
    {
        var payQuery = await _accountPaymentRequestRepository.GetQueryableAsync();
        var latest = await AsyncExecuter.FirstOrDefaultAsync(
            payQuery
                .Where(p => p.PolicyId == policyId)
                .OrderByDescending(p => p.CreationTime));

        if (latest == null)
        {
            return;
        }

        if (latest.Status != AccountPaymentRequestStatus.PendingApproval &&
            latest.Status != AccountPaymentRequestStatus.Rejected)
        {
            return;
        }

        HrEmployee? approver = null;
        if (CurrentUser.Id.HasValue)
        {
            approver = await _hrEmployeeRepository.FirstOrDefaultAsync(e => e.UserId == CurrentUser.Id.Value);
        }

        if (approver != null)
        {
            latest.Approve(Clock.Now, approver.Id);
        }
        else
        {
            latest.UpdateStatus(AccountPaymentRequestStatus.Approved);
        }

        await _accountPaymentRequestRepository.UpdateAsync(latest);
    }

    private async Task TrySubmitDraftPolicyForApprovalAsync(PolicyEntity policy)
    {
        var newestVersion = policy.PolicyVersions
            .FirstOrDefault(v => v.Id == policy.LastVersionId);
        if (newestVersion == null)
        {
            return;
        }

        if (ParseVersionStatusToPolicyStatus(newestVersion.Status) != PolicyStatus.Draft)
        {
            return;
        }

        await _policyAppService.SubmitForApprovalAsync(policy.Id, new SubmitForApprovalInput());
    }

    private static PolicyStatus ParseVersionStatusToPolicyStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return PolicyStatus.Draft;
        }

        var lower = status.Trim().ToLowerInvariant();
        return lower switch
        {
            "quotation" => PolicyStatus.Quotation,
            "draft" => PolicyStatus.Draft,
            "active" => PolicyStatus.Active,
            "expired" => PolicyStatus.Expired,
            "terminated" => PolicyStatus.Terminated,
            "cancelled" => PolicyStatus.Cancelled,
            _ => Enum.TryParse<PolicyStatus>(status, true, out var parsed) ? parsed : PolicyStatus.Draft
        };
    }

    public async Task<PartnerCreatePolicyResultDto> PartnerCreatePolicyAsync(PartnerCreatePolicyInput input)
    {
        if (string.IsNullOrWhiteSpace(input.PolicyVersionId) || !Guid.TryParse(input.PolicyVersionId.Trim(), out var policyVersionId))
            throw new BusinessException("Policy:PartnerCreatePolicy:InvalidPolicyId");

        var policyVersion = await _policyVersionRepository.FindAsync(policyVersionId);
        if (policyVersion == null)
            throw new BusinessException("Policy:PartnerCreatePolicy:PolicyNotFound");

        var query = await _policyRepository.GetQueryableAsync();
        var policy = await query
            .Include(p => p.Partner)
            .Include(p => p.PolicyVersions)
            .ThenInclude(v => v.PolicyProducts)
            .ThenInclude(pp => pp.Product)
            .ThenInclude(p => p!.ProductType)
            .Include(p => p.PolicyVersions)
            .ThenInclude(v => v.PolicyRiskObjects)
            .ThenInclude(ro => ro.PolicyRiskMotors)
            .Where(p => p.Id == policyVersion.PolicyId)
            .FirstOrDefaultAsync();

        if (policy == null)
            throw new BusinessException("Policy:PartnerCreatePolicy:PolicyNotFound");

        var results = await _partnerIntegrationService.IssuePolicyAsync(policy);

        var errors = results
            .Where(r => !r.IsSuccess && !string.IsNullOrEmpty(r.ErrorMessage))
            .Select(r => r.ErrorMessage!)
            .ToList();

        if (errors.Count > 0)
        {
            var currentVersion = policy.PolicyVersions
                .FirstOrDefault(v => v.Id == policyVersionId);
            if (currentVersion != null)
            {
                currentVersion.UpdateInsurerIntegrationStatus("failed");
                currentVersion.UpdateInsurerIntegrationDescription(string.Join("; ", errors));
                await _policyVersionRepository.UpdateAsync(currentVersion);
            }
        }

        return new PartnerCreatePolicyResultDto
        {
            Success = errors.Count == 0,
            ErrorMessages = errors
        };
    }
}
