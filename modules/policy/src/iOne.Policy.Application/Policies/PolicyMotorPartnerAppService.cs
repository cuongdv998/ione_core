using System;
using System.Linq;
using System.Threading.Tasks;
using iOne.AccountPaymentRequests;
using iOne.AdminConfigs;
using iOne.HrEmployees;
using iOne.PartnerIntegration;
using iOne.Policy.Localization;
using iOne.Policy.Payments;
using iOne.ProLineOfBusinesses;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using PolicyEntity = iOne.Policies.Policy;

namespace iOne.Policy.Policies;

public class PolicyMotorPartnerAppService : ApplicationService, IPolicyMotorPartnerAppService
{
    private const string MotorbikeLobAdminConfigCode = "DEFAULT_LOB_MOTORBIKE";
    private const string MotorbikeInitialPaymentMethodCode = "CK";

    private readonly IRepository<PolicyEntity, Guid> _policyRepository;
    private readonly IRepository<AdminConfig, Guid> _adminConfigRepository;
    private readonly IRepository<ProLineOfBusiness, Guid> _proLineOfBusinessRepository;
    private readonly IRepository<HrEmployee, Guid> _hrEmployeeRepository;
    private readonly IPartnerIntegrationService _partnerIntegrationService;
    private readonly PolicyAppService _policyAppService;

    public PolicyMotorPartnerAppService(
        IRepository<PolicyEntity, Guid> policyRepository,
        IRepository<AdminConfig, Guid> adminConfigRepository,
        IRepository<ProLineOfBusiness, Guid> proLineOfBusinessRepository,
        IRepository<HrEmployee, Guid> hrEmployeeRepository,
        IPartnerIntegrationService partnerIntegrationService,
        PolicyAppService policyAppService)
    {
        _policyRepository = policyRepository;
        _adminConfigRepository = adminConfigRepository;
        _proLineOfBusinessRepository = proLineOfBusinessRepository;
        _hrEmployeeRepository = hrEmployeeRepository;
        _partnerIntegrationService = partnerIntegrationService;
        _policyAppService = policyAppService;
        LocalizationResource = typeof(PolicyResource);
    }

    public async Task<PartnerMotorPolicyIssueResultDto> IssueMotorPolicyAsync(PartnerMotorPolicyIssueInput input)
    {
        // 1. Resolve motorbike LOB id from admin_config DEFAULT_LOB_MOTORBIKE
        var adminConfigQuery = await _adminConfigRepository.GetQueryableAsync();
        var motorbikeLobConfig = await AsyncExecuter.FirstOrDefaultAsync(
            adminConfigQuery.Where(x =>
                x.Code == MotorbikeLobAdminConfigCode &&
                x.Status == AdminConfigStatus.Active));

        var motorbikeLobCode = motorbikeLobConfig?.Value?.Trim();
        if (string.IsNullOrWhiteSpace(motorbikeLobCode))
            throw new BusinessException("Policy:MotorPartnerIssue:MotorbikeLobNotConfigured")
                .WithData("AdminConfigCode", MotorbikeLobAdminConfigCode);

        // 2. Look up the ProLineOfBusiness GUID for that code
        var lobQuery = await _proLineOfBusinessRepository.GetQueryableAsync();
        var motorbikeLob = await AsyncExecuter.FirstOrDefaultAsync(
            lobQuery.Where(x => x.Code == motorbikeLobCode));
        if (motorbikeLob == null)
            throw new BusinessException("Policy:MotorPartnerIssue:MotorbikeLobNotFound")
                .WithData("LobCode", motorbikeLobCode);

        // 3. Load policy with the full graph the strategy needs
        var policyQuery = await _policyRepository.GetQueryableAsync();
        var policy = await AsyncExecuter.FirstOrDefaultAsync(
            policyQuery
                .Include(p => p.Partner)
                .Include(p => p.Contract)
                    .ThenInclude(c => c!.Customer)
                .Include(p => p.PolicyVersions)
                    .ThenInclude(v => v.PolicyProducts)
                        .ThenInclude(pp => pp.Product)
                            .ThenInclude(pr => pr!.ProductType)
                .Include(p => p.PolicyVersions)
                    .ThenInclude(v => v.PolicyRiskObjects)
                        .ThenInclude(ro => ro.PolicyRiskMotors)
                .Where(p => p.Id == input.PolicyId && !p.IsDeleted));

        if (policy == null)
            throw new BusinessException("Policy:MotorPartnerIssue:PolicyNotFound")
                .WithData("PolicyId", input.PolicyId);

        // 4. Enforce motorbike LOB — this endpoint is explicitly for motor policies
        if (policy.LobId != motorbikeLob.Id)
            throw new BusinessException("Policy:MotorPartnerIssue:PolicyIsNotMotorbike")
                .WithData("PolicyId", input.PolicyId)
                .WithData("ExpectedLobCode", motorbikeLobCode);

        // 5. Resolve submitter from current user
        if (CurrentUser.Id == null)
            throw new UserFriendlyException(L["UserNotAuthenticated"].Value);
        var submitter = await _hrEmployeeRepository.FirstOrDefaultAsync(e => e.UserId == CurrentUser.Id.Value);
        if (submitter == null)
            throw new UserFriendlyException(L["EmployeeNotFoundForUser"].Value);

        // 6. Map BillInfo DTO to partner context model
        PartnerIssueBillInfo? billInfo = null;
        if (input.BillInfo != null)
        {
            billInfo = new PartnerIssueBillInfo
            {
                IsChoice  = input.BillInfo.IsChoice,
                BillActor = input.BillInfo.BillActor,
                Email     = input.BillInfo.Email
            };
        }

        // 7. Map OwnerInfo DTO to partner context model
        PartnerIssueOwnerInfo? ownerInfo = null;
        if (input.OwnerInfo != null)
        {
            ownerInfo = new PartnerIssueOwnerInfo
            {
                IsCustomer = input.OwnerInfo.IsCustomer,
                FullName   = input.OwnerInfo.FullName,
                Phone      = input.OwnerInfo.Phone,
                Address    = input.OwnerInfo.Address,
                Email      = input.OwnerInfo.Email
            };
        }

        // 8. Call the partner integration service FIRST (throws on failure → UoW rolled back by ABP)
        var result = await _partnerIntegrationService.IssueMotorPolicyAsync(policy, billInfo, ownerInfo);

        // 8. Persist transId → Policy.InsurerPolicyNo
        if (!string.IsNullOrWhiteSpace(result.TaskId))
            policy.UpdateInsurerPolicyNo(result.TaskId);

        // 9. Persist contractNumber → PolicyContract.InsurerContractCode
        if (policy.Contract != null && !string.IsNullOrWhiteSpace(result.ContractNumber))
            policy.Contract.UpdateInsurerContractCode(result.ContractNumber);

        // 10. Create AccountPaymentRequest AFTER successful partner call (do not run FIFO — policy_amount stays new).
        var paymentInput = new CreatePaymentRequestInput
        {
            PaymentMethodCode = MotorbikeInitialPaymentMethodCode,
            Amount            = policy.PremiumTotal,
            PaymentDate       = Clock.Now
        };
        await _policyAppService.CreatePaymentRequestCoreAsync(
            policy.Id,
            paymentInput,
            AccountPaymentRequestStatus.PendingApproval,
            submitter.Id,
            applyFifoToPolicyAmounts: false);

        return new PartnerMotorPolicyIssueResultDto
        {
            PartnerResponse = result.PartnerResponse.Count > 0 ? result.PartnerResponse : null
        };
    }
}
