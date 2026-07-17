using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using iOne.HrEmployees;
using iOne.Policies;
using iOne.Policy.Localization;
using iOne.Policy.Permissions;
using iOne.Policy.Policies;
using iOne.Policy.PolicyRequestApproval;
using iOne.PolicyContracts;
using iOne.ProProducts;
using iOne.ResChannels;
using iOne.WorkInstances;
using iOne.WorkTasks;
using iOne.Workflow;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;

namespace iOne.Policy.PolicyRequestApproval;

[RemoteService(Name = PolicyRemoteServiceConsts.RemoteServiceName)]
public class PolicyRequestApprovalAppService : ApplicationService, IPolicyRequestApprovalAppService
{
    private const string CreatePolicyApprovalBusinessCode = "CREATE_POLICY_APPROVAL";
    private const string TerminatePolicyApprovalBusinessCode = "TERMINATE_POLICY_APPROVAL";
    private const string EndorsementPolicyApprovalBusinessCode = "ENDORSEMENT_POLICY_APPROVAL";
    private const string PolicyBusinessName = "policyVersion";

    private readonly IPolicyRequestApprovalQueryRepository _approvalQueryRepository;
    private readonly IRepository<iOne.Policies.Policy, Guid> _policyRepository;
    private readonly IRepository<PolicyVersion, Guid> _policyVersionRepository;
    private readonly IWorkTaskRepository _workTaskRepository;
    private readonly IRepository<HrEmployee, Guid> _hrEmployeeRepository;
    private readonly IRepository<IdentityUser, Guid> _userRepository;
    private readonly IResChannelRepository _resChannelRepository;
    private readonly IRepository<ProProduct, Guid> _proProductRepository;
    private readonly IPermissionChecker _permissionChecker;
    private readonly IWorkInstanceRepository _workInstanceRepository;
    private readonly IElsaWorkflowService _elsaWorkflowService;

    public PolicyRequestApprovalAppService(
        IPolicyRequestApprovalQueryRepository approvalQueryRepository,
        IRepository<iOne.Policies.Policy, Guid> policyRepository,
        IRepository<PolicyVersion, Guid> policyVersionRepository,
        IWorkTaskRepository workTaskRepository,
        IRepository<HrEmployee, Guid> hrEmployeeRepository,
        IRepository<IdentityUser, Guid> userRepository,
        IResChannelRepository resChannelRepository,
        IRepository<ProProduct, Guid> proProductRepository,
        IPermissionChecker permissionChecker,
        IWorkInstanceRepository workInstanceRepository,
        IElsaWorkflowService elsaWorkflowService)
    {
        _approvalQueryRepository = approvalQueryRepository;
        _policyRepository = policyRepository;
        _policyVersionRepository = policyVersionRepository;
        _workTaskRepository = workTaskRepository;
        _hrEmployeeRepository = hrEmployeeRepository;
        _userRepository = userRepository;
        _resChannelRepository = resChannelRepository;
        _proProductRepository = proProductRepository;
        _permissionChecker = permissionChecker;
        _workInstanceRepository = workInstanceRepository;
        _elsaWorkflowService = elsaWorkflowService;
        LocalizationResource = typeof(PolicyResource);
    }

    public async Task<PagedResultDto<PolicyRequestApprovalItemDto>> GetListAsync(GetPolicyRequestApprovalListInput input)
    {
        var currentUserId = CurrentUser.Id ?? throw new BusinessException("Policy:PolicyRequestApproval:NotAuthenticated");
        var employee = await _hrEmployeeRepository.FirstOrDefaultAsync(e => e.UserId == currentUserId);
        var currentEmployeeId = employee?.Id ?? Guid.Empty;

        var effectiveCodes = ResolveEffectiveBusinessCodes(input);
        if (effectiveCodes != null && effectiveCodes.Count > 0)
        {
            foreach (var code in effectiveCodes)
                await EnsurePermissionForBusinessCodeAsync(code, view: true);
        }
        else
        {
            await EnsureAnyApprovalViewPermissionAsync();
        }

        var baseQuery = await _approvalQueryRepository.GetApprovalListWithCodeQueryableAsync(currentEmployeeId, effectiveCodes);
        var query = ApplyFiltersOnPair(baseQuery, input);
        var totalCount = await AsyncExecuter.CountAsync(query);

        query = ApplySortingOnPair(query, input);
        query = ApplyPagingOnPair(query, input);
        var orderedRows = await AsyncExecuter.ToListAsync(query.Select(x => new { x.Policy.Id, x.PolicyVersionId, x.BusinessCode, x.WorkTaskId, x.WorkTaskStatus }));

        if (orderedRows.Count == 0)
            return new PagedResultDto<PolicyRequestApprovalItemDto>(totalCount, new List<PolicyRequestApprovalItemDto>());

        var policyIds = orderedRows.Select(x => x.Id).Distinct().ToList();
        var policyQuery = await _policyRepository.GetQueryableAsync();
        policyQuery = policyQuery
            .Where(p => policyIds.Contains(p.Id))
            .Include(x => x.Contract)
            .ThenInclude(c => c!.Customer)
            .Include(x => x.PolicyVersions)
            .ThenInclude(v => v.PolicyProducts)
            .Include(x => x.PolicyVersions)
            .ThenInclude(v => v.PolicyRiskObjects)
            .ThenInclude(ro => ro.PolicyRiskMotors)
            .Include(x => x.PolicyVersions)
            .ThenInclude(v => v.PolicyCertificates);
        var entities = await AsyncExecuter.ToListAsync(policyQuery);
        var policyById = entities.ToDictionary(p => p.Id);
        var orderedPairs = orderedRows.Select(x => (policyById[x.Id], x.PolicyVersionId, x.BusinessCode, x.WorkTaskId, x.WorkTaskStatus)).ToList();

        var implementerIds = orderedPairs.Select(x => x.Item1.ImplementerId).Distinct().ToList();
        var implementerNameById = new System.Collections.Generic.Dictionary<Guid, string>();
        if (implementerIds.Count > 0)
        {
            var empQuery = await _hrEmployeeRepository.GetQueryableAsync();
            var empPairs = await AsyncExecuter.ToListAsync(
                empQuery.Where(e => implementerIds.Contains(e.Id)).Select(e => new { e.Id, e.FullName }));
            implementerNameById = empPairs.GroupBy(x => x.Id).ToDictionary(g => g.Key, g => g.First().FullName ?? string.Empty);
        }

        var creatorIds = orderedPairs
            .Select(x => x.Item1.CreatorId)
            .Where(id => id.HasValue && id.Value != Guid.Empty)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();
        var creatorNameById = new System.Collections.Generic.Dictionary<Guid, string>();
        if (creatorIds.Count > 0)
        {
            var userQuery = await _userRepository.GetQueryableAsync();
            var users = await AsyncExecuter.ToListAsync(
                userQuery.Where(u => creatorIds.Contains(u.Id)).Select(u => new { u.Id, u.UserName, u.Name, u.Surname }));
            creatorNameById = users.GroupBy(x => x.Id).ToDictionary(g => g.Key, g =>
            {
                var u = g.First();
                var fullName = $"{u.Name} {u.Surname}".Trim();
                return !string.IsNullOrWhiteSpace(fullName) ? fullName : (u.UserName ?? string.Empty);
            });
        }

        var channelIds = orderedPairs.Select(x => x.Item1.ChannelId).Where(id => id.HasValue && id.Value != Guid.Empty).Select(id => id!.Value).Distinct().ToList();
        var channelNameById = new System.Collections.Generic.Dictionary<Guid, string>();
        if (channelIds.Count > 0)
        {
            var chQuery = await _resChannelRepository.GetQueryableAsync();
            var chPairs = await AsyncExecuter.ToListAsync(chQuery.Where(c => channelIds.Contains(c.Id)).Select(c => new { c.Id, c.Name }));
            channelNameById = chPairs.GroupBy(x => x.Id).ToDictionary(g => g.Key, g => g.First().Name ?? string.Empty);
        }

        var allProductIds = orderedPairs
            .SelectMany(p => p.Item1.PolicyVersions.SelectMany(v => v.PolicyProducts.Select(pp => pp.ProductId)))
            .Distinct()
            .ToList();
        var productNameById = new System.Collections.Generic.Dictionary<Guid, string>();
        if (allProductIds.Count > 0)
        {
            var productQuery = await _proProductRepository.GetQueryableAsync();
            var productPairs = await AsyncExecuter.ToListAsync(productQuery.Where(p => allProductIds.Contains(p.Id)).Select(p => new { p.Id, p.Name }));
            productNameById = productPairs.GroupBy(x => x.Id).ToDictionary(g => g.Key, g => g.First().Name ?? string.Empty);
        }

        var items = orderedPairs.Select(pair =>
        {
            var entity = pair.Item1;
            var policyVersionId = pair.Item2;
            var businessCode = pair.Item3;
            var workTaskId = pair.Item4;
            var workTaskStatus = pair.Item5;
            var dto = ObjectMapper.Map<iOne.Policies.Policy, PolicyDto>(entity);
            dto.ContractNo = entity.Contract?.Code;
            dto.CustomerId = entity.Contract?.CustomerId;
            dto.CustomerName = entity.Contract?.Customer?.Name;
            if (implementerNameById.TryGetValue(entity.ImplementerId, out var impName) && !string.IsNullOrWhiteSpace(impName))
            {
                dto.ImplementerName = impName;
                dto.PolicyIssuerName = impName;
                dto.PolicyIssuerId = entity.ImplementerId;
            }
            if (entity.CreatorId.HasValue && creatorNameById.TryGetValue(entity.CreatorId.Value, out var creatorName) && !string.IsNullOrWhiteSpace(creatorName))
                dto.CreatorName = creatorName;
            if (entity.ChannelId.HasValue && channelNameById.TryGetValue(entity.ChannelId.Value, out var channelName) && !string.IsNullOrWhiteSpace(channelName))
                dto.ChannelName = channelName;
            // Version đang chờ phê duyệt (theo work task); nếu không tìm thấy (version đã xóa) thì fallback version mới nhất
            var targetVersion = entity.PolicyVersions.FirstOrDefault(v => v.Id == policyVersionId)
                ?? entity.PolicyVersions.OrderByDescending(v => v.Version).FirstOrDefault();
            if (targetVersion != null)
            {
                var names = targetVersion.PolicyProducts
                    .Select(pp => productNameById.TryGetValue(pp.ProductId, out var name) ? name : string.Empty)
                    .Where(n => !string.IsNullOrWhiteSpace(n))
                    .Distinct()
                    .ToList();
                dto.ProductName = names.Count > 0 ? string.Join(", ", names) : null;
                var latestMotor = targetVersion.PolicyRiskObjects.SelectMany(ro => ro.PolicyRiskMotors).FirstOrDefault();
                dto.VehiclePlate = latestMotor?.CarPlate;
                dto.ChassisNumber = latestMotor?.CarVin;
                dto.EngineNumber = latestMotor?.CarEngineNumber;
                dto.OrgEffectDate = targetVersion.EffectDate;
                dto.OrgExpireDate = targetVersion.ExpireDate;
                dto.PremiumTotal = PolicyNetPremiumForListDisplay.Compute(
                    targetVersion.PremiumTotal,
                    targetVersion.Discount,
                    targetVersion.Markup);
                dto.Premium = targetVersion.Premium;
                dto.Vat = targetVersion.Vat;
                dto.ApprovalDate = targetVersion.ApprovalDate;
                var certForVersion = targetVersion.PolicyCertificates?.FirstOrDefault(c => !c.IsDeleted);
                dto.CertificateNo = certForVersion?.CertificateNo;
            }
            dto.TerminationStatus = targetVersion?.TerminationStatus;
            return new PolicyRequestApprovalItemDto { Policy = dto, PolicyVersionId = policyVersionId, BusinessCode = businessCode, WorkTaskId = workTaskId, WorkTaskStatus = workTaskStatus, PolicyVersionStatus = targetVersion?.Status };
        }).ToList();

        return new PagedResultDto<PolicyRequestApprovalItemDto>(totalCount, items);
    }

    /// <summary>
    /// Resolves the effective list of business codes from input. When BusinessCodes is non-null and non-empty, use it;
    /// when null or empty, use legacy BusinessCode as single-element list; when both are empty, return null (no filter).
    /// </summary>
    private static IReadOnlyList<string>? ResolveEffectiveBusinessCodes(GetPolicyRequestApprovalListInput input)
    {
        if (input.BusinessCodes != null && input.BusinessCodes.Count > 0)
            return input.BusinessCodes.Where(c => !string.IsNullOrWhiteSpace(c)).Select(c => c!.Trim()).ToList();
        if (!string.IsNullOrWhiteSpace(input.BusinessCode))
            return new[] { input.BusinessCode.Trim() };
        return null;
    }

    private async Task EnsureAnyApprovalViewPermissionAsync()
    {
        var requestView = await _permissionChecker.IsGrantedAsync(PolicyPermissions.RequestApproval.View);
        var terminateView = await _permissionChecker.IsGrantedAsync(PolicyPermissions.TerminateApproval.View);
        var endorsementView = await _permissionChecker.IsGrantedAsync(PolicyPermissions.EndorsementApproval.View);
        if (!requestView && !terminateView && !endorsementView)
            throw new Volo.Abp.Authorization.AbpAuthorizationException(PolicyPermissions.RequestApproval.View);
    }

    public async Task<PolicyRequestApprovalItemDto> GetByWorkTaskIdAsync(Guid workTaskId)
    {
        var currentUserId = CurrentUser.Id ?? throw new BusinessException("Policy:PolicyRequestApproval:NotAuthenticated");
        var employee = await _hrEmployeeRepository.FirstOrDefaultAsync(e => e.UserId == currentUserId);
        var currentEmployeeId = employee?.Id ?? Guid.Empty;

        var workTask = await _workTaskRepository.GetAsync(workTaskId);
        if (workTask.BusinessName != PolicyBusinessName)
            throw new BusinessException("Policy:PolicyRequestApproval:NotAuthorized");
        if (workTask.AssigneeId != currentEmployeeId)
            throw new BusinessException("Policy:PolicyRequestApproval:NotAuthorized");

        await EnsurePermissionForBusinessCodeAsync(workTask.BusinessCode, view: true);

        var policyVersion = await _policyVersionRepository.FindAsync(workTask.BusinessKey);
        if (policyVersion == null)
            throw new BusinessException("Policy:PolicyRequestApproval:NotAuthorized");

        var policyQuery = await _policyRepository.GetQueryableAsync();
        var policy = await AsyncExecuter.FirstOrDefaultAsync(policyQuery
            .Where(p => p.Id == policyVersion.PolicyId)
            .Include(x => x.Contract)
            .ThenInclude(c => c!.Customer)
            .Include(x => x.PolicyVersions)
            .ThenInclude(v => v.PolicyProducts)
            .Include(x => x.PolicyVersions)
            .ThenInclude(v => v.PolicyRiskObjects)
            .ThenInclude(ro => ro.PolicyRiskMotors));
        if (policy == null)
            throw new BusinessException("Policy:PolicyRequestApproval:NotAuthorized");

        var dto = ObjectMapper.Map<iOne.Policies.Policy, PolicyDto>(policy);
        dto.ContractNo = policy.Contract?.Code;
        dto.CustomerId = policy.Contract?.CustomerId;
        dto.CustomerName = policy.Contract?.Customer?.Name;
        if (policy.ImplementerId != Guid.Empty)
        {
            var implementer = await _hrEmployeeRepository.FindAsync(policy.ImplementerId);
            if (implementer != null)
            {
                dto.ImplementerName = implementer.FullName;
                dto.PolicyIssuerName = implementer.FullName;
                dto.PolicyIssuerId = policy.ImplementerId;
            }
        }
        if (policy.CreatorId.HasValue)
        {
            var creator = await _userRepository.FindAsync(policy.CreatorId.Value);
            if (creator != null)
                dto.CreatorName = $"{creator.Name} {creator.Surname}".Trim();
        }
        if (policy.ChannelId.HasValue)
        {
            var channel = await _resChannelRepository.FindAsync(policy.ChannelId.Value);
            if (channel != null)
                dto.ChannelName = channel.Name;
        }
        // Dữ liệu theo version đang chờ phê duyệt (work task.BusinessKey = policyVersion.Id), không dùng version mới nhất
        var targetVersion = policy.PolicyVersions.FirstOrDefault(v => v.Id == policyVersion.Id);
        if (targetVersion != null)
        {
            var productIds = targetVersion.PolicyProducts.Select(pp => pp.ProductId).Distinct().ToList();
            if (productIds.Count > 0)
            {
                var productQuery = await _proProductRepository.GetQueryableAsync();
                var productNames = await AsyncExecuter.ToListAsync(productQuery.Where(p => productIds.Contains(p.Id)).Select(p => p.Name));
                dto.ProductName = productNames.Count > 0 ? string.Join(", ", productNames.Where(n => !string.IsNullOrWhiteSpace(n))) : null;
            }
            var latestMotor = targetVersion.PolicyRiskObjects.SelectMany(ro => ro.PolicyRiskMotors).FirstOrDefault();
            dto.VehiclePlate = latestMotor?.CarPlate;
            dto.ChassisNumber = latestMotor?.CarVin;
            dto.EngineNumber = latestMotor?.CarEngineNumber;
            dto.OrgEffectDate = targetVersion.EffectDate;
            dto.OrgExpireDate = targetVersion.ExpireDate;
            dto.PremiumTotal = PolicyNetPremiumForListDisplay.Compute(
                targetVersion.PremiumTotal,
                targetVersion.Discount,
                targetVersion.Markup);
            dto.Premium = targetVersion.Premium;
            dto.Vat = targetVersion.Vat;
            dto.TerminationStatus = targetVersion.TerminationStatus;

            dto.VersionDetail = new PolicyVersionDetailDto
            {
                Version = targetVersion.Version,
                EffectDate = targetVersion.EffectDate,
                ExpireDate = targetVersion.ExpireDate,
                OrgEffectDate = targetVersion.OrgEffectDate,
                OrgExpireDate = targetVersion.OrgExpireDate,
                InternalNote = targetVersion.InternalNote,
                CustomerNote = targetVersion.CustomerNote,
                PremiumTotal = targetVersion.PremiumTotal,
                Premium = targetVersion.Premium,
                Vat = targetVersion.Vat,
                Discount = targetVersion.Discount,
                DiscountRate = targetVersion.DiscountRate,
                Markup = targetVersion.Markup,
                EndorsementType = targetVersion.EndorsementType,
                EndorsementReasonId = targetVersion.EndorsementReasonId,
                EndorsementDescription = targetVersion.EndorsementDescription,
                RefundAmount = targetVersion.RefundAmount
            };
        }
        return new PolicyRequestApprovalItemDto { Policy = dto, PolicyVersionId = workTask.BusinessKey, BusinessCode = workTask.BusinessCode, WorkTaskId = workTask.Id, WorkTaskStatus = workTask.Status };
    }

    public async Task ApproveAsync(Guid workTaskId)
    {
        var currentUserId = CurrentUser.Id ?? throw new UserFriendlyException("Policy:PolicyRequestApproval:NotAuthenticated");
        var employee = await _hrEmployeeRepository.FirstOrDefaultAsync(e => e.UserId == currentUserId);
        var currentEmployeeId = employee?.Id ?? Guid.Empty;

        var workTask = await _workTaskRepository.GetAsync(workTaskId);
        if (workTask.BusinessName != PolicyBusinessName || workTask.AssigneeId != currentEmployeeId)
            throw new UserFriendlyException("Policy:PolicyRequestApproval:NotAuthorized");
        if (workTask.Status != WorkTaskStatus.WaitApprove)
            throw new UserFriendlyException("Policy:PolicyRequestApproval:NotAuthorized");

        await EnsurePermissionForBusinessCodeAsync(workTask.BusinessCode, view: false);

        workTask.UpdateStatus(WorkTaskStatus.Approved);
        await _workTaskRepository.UpdateAsync(workTask);

        if (workTask.WorkInstanceId != null)
        {
            var workInstance = await _workInstanceRepository.GetAsync(workTask.WorkInstanceId.Value);
            var eventName = workTask.EventName ?? workInstance.WorkflowInstanceId;
            await _elsaWorkflowService.TriggerApprovalAsync(eventName, workInstance.WorkflowInstanceId, "approve", null);
        }
    }

    public async Task RejectAsync(Guid workTaskId, RejectPolicyRequestInput input)
    {
        if (string.IsNullOrWhiteSpace(input.ReasonDescription))
            throw new UserFriendlyException("Policy:PolicyRequestApproval:ReasonDescriptionRequired");

        var currentUserId = CurrentUser.Id ?? throw new UserFriendlyException("Policy:PolicyRequestApproval:NotAuthenticated");
        var employee = await _hrEmployeeRepository.FirstOrDefaultAsync(e => e.UserId == currentUserId);
        var currentEmployeeId = employee?.Id ?? Guid.Empty;

        var workTask = await _workTaskRepository.GetAsync(workTaskId);
        if (workTask.BusinessName != PolicyBusinessName || workTask.AssigneeId != currentEmployeeId)
            throw new UserFriendlyException("Policy:PolicyRequestApproval:NotAuthorized");
        if (workTask.Status != WorkTaskStatus.WaitApprove)
            throw new UserFriendlyException("Policy:PolicyRequestApproval:NotAuthorized");

        await EnsurePermissionForBusinessCodeAsync(workTask.BusinessCode, view: false);

        var rejectVersion = await _policyVersionRepository.GetAsync(workTask.BusinessKey);
        var policy = await _policyRepository.GetAsync(rejectVersion.PolicyId);
        policy.UpdateApprovalStatus("rejected");
        await _policyRepository.UpdateAsync(policy);

        workTask.UpdateRejectionReason(input.ReasonId, input.ReasonDescription?.Trim());
        workTask.UpdateStatus(WorkTaskStatus.Rejected);
        await _workTaskRepository.UpdateAsync(workTask);

        if (workTask.WorkInstanceId != null)
        {
            var workInstance = await _workInstanceRepository.GetAsync(workTask.WorkInstanceId.Value);
            workInstance.UpdateStatus(WorkInstanceStatus.Rejected);
            await _workInstanceRepository.UpdateAsync(workInstance);
            var eventName = workTask.EventName ?? workInstance.WorkflowInstanceId;
            await _elsaWorkflowService.TriggerApprovalAsync(eventName, workInstance.WorkflowInstanceId, "reject", null);
        }
    }

    public async Task ApproveBatchAsync(ApproveBatchRequest input)
    {
        if (input.WorkTaskIds == null || input.WorkTaskIds.Count == 0)
            throw new UserFriendlyException("Policy:PolicyRequestApproval:SelectAtLeastOne");

        var currentUserId = CurrentUser.Id ?? throw new UserFriendlyException("Policy:PolicyRequestApproval:NotAuthenticated");
        var employee = await _hrEmployeeRepository.FirstOrDefaultAsync(e => e.UserId == currentUserId);
        var currentEmployeeId = employee?.Id ?? Guid.Empty;

        var workTaskQuery = await _workTaskRepository.GetQueryableAsync();
        var workTasks = await AsyncExecuter.ToListAsync(workTaskQuery
            .Where(w => input.WorkTaskIds.Contains(w.Id)
                && w.BusinessName == PolicyBusinessName
                && w.AssigneeId == currentEmployeeId
                && w.Status == WorkTaskStatus.WaitApprove));

        if (workTasks.Count != input.WorkTaskIds.Count)
        {
            var foundIds = workTasks.Select(w => w.Id).ToHashSet();
            var invalidIds = input.WorkTaskIds.Where(id => !foundIds.Contains(id)).ToList();
            throw new UserFriendlyException("Policy:PolicyRequestApproval:OneOrMoreTasksNotValidForApproval")
                .WithData("InvalidCount", input.WorkTaskIds.Count - workTasks.Count)
                .WithData("InvalidIds", string.Join(", ", invalidIds));
        }

        foreach (var workTask in workTasks)
            await EnsurePermissionForBusinessCodeAsync(workTask.BusinessCode, view: false);

        foreach (var workTask in workTasks)
            workTask.UpdateStatus(WorkTaskStatus.Approved);

        await CurrentUnitOfWork.SaveChangesAsync();

        var elsaTriggers = new List<(string EventName, string WorkflowInstanceId, string Action)>();
        var workInstanceIds = workTasks.Where(w => w.WorkInstanceId != null).Select(w => w.WorkInstanceId!.Value).Distinct().ToList();
        if (workInstanceIds.Count > 0)
        {
            var wiQuery = await _workInstanceRepository.GetQueryableAsync();
            var workInstances = await AsyncExecuter.ToListAsync(wiQuery.Where(wi => workInstanceIds.Contains(wi.Id)));
            var wiById = workInstances.ToDictionary(wi => wi.Id);
            foreach (var workTask in workTasks.Where(w => w.WorkInstanceId != null))
            {
                if (wiById.TryGetValue(workTask.WorkInstanceId!.Value, out var workInstance))
                {
                    var eventName = workTask.EventName ?? workInstance.WorkflowInstanceId;
                    elsaTriggers.Add((eventName, workInstance.WorkflowInstanceId, "approve"));
                }
            }
        }

        TriggerElsaBatchInBackground(elsaTriggers);
    }

    public async Task RejectBatchAsync(RejectBatchRequest input)
    {
        if (string.IsNullOrWhiteSpace(input.ReasonDescription))
            throw new UserFriendlyException("Policy:PolicyRequestApproval:ReasonDescriptionRequired");
        if (input.WorkTaskIds == null || input.WorkTaskIds.Count == 0)
            throw new UserFriendlyException("Policy:PolicyRequestApproval:SelectAtLeastOne");

        var currentUserId = CurrentUser.Id ?? throw new UserFriendlyException("Policy:PolicyRequestApproval:NotAuthenticated");
        var employee = await _hrEmployeeRepository.FirstOrDefaultAsync(e => e.UserId == currentUserId);
        var currentEmployeeId = employee?.Id ?? Guid.Empty;

        var workTaskQuery = await _workTaskRepository.GetQueryableAsync();
        var workTasks = await AsyncExecuter.ToListAsync(workTaskQuery
            .Where(w => input.WorkTaskIds.Contains(w.Id)
                && w.BusinessName == PolicyBusinessName
                && w.AssigneeId == currentEmployeeId
                && w.Status == WorkTaskStatus.WaitApprove));

        if (workTasks.Count != input.WorkTaskIds.Count)
        {
            var foundIds = workTasks.Select(w => w.Id).ToHashSet();
            var invalidIds = input.WorkTaskIds.Where(id => !foundIds.Contains(id)).ToList();
            throw new UserFriendlyException("Policy:PolicyRequestApproval:OneOrMoreTasksNotValidForRejection")
                .WithData("InvalidCount", input.WorkTaskIds.Count - workTasks.Count)
                .WithData("InvalidIds", string.Join(", ", invalidIds));
        }

        foreach (var workTask in workTasks)
            await EnsurePermissionForBusinessCodeAsync(workTask.BusinessCode, view: false);

        var policyVersionIds = workTasks.Select(w => w.BusinessKey).Distinct().ToList();
        var policyVersions = await AsyncExecuter.ToListAsync((await _policyVersionRepository.GetQueryableAsync()).Where(pv => policyVersionIds.Contains(pv.Id)));
        var policyIds = policyVersions.Select(pv => pv.PolicyId).Distinct().ToList();
        var policies = await AsyncExecuter.ToListAsync((await _policyRepository.GetQueryableAsync()).Where(p => policyIds.Contains(p.Id)));
        var policyById = policies.ToDictionary(p => p.Id);
        var pvById = policyVersions.ToDictionary(pv => pv.Id);

        var workInstanceIds = workTasks.Where(w => w.WorkInstanceId != null).Select(w => w.WorkInstanceId!.Value).Distinct().ToList();
        var workInstances = workInstanceIds.Count > 0
            ? await AsyncExecuter.ToListAsync((await _workInstanceRepository.GetQueryableAsync()).Where(wi => workInstanceIds.Contains(wi.Id)))
            : new List<WorkInstance>();
        var wiById = workInstances.ToDictionary(wi => wi.Id);

        var reasonId = input.ReasonId;
        var reasonDescription = (input.ReasonDescription ?? string.Empty).Trim();

        foreach (var workTask in workTasks)
        {
            if (pvById.TryGetValue(workTask.BusinessKey, out var rejectVersion) && policyById.TryGetValue(rejectVersion.PolicyId, out var policy))
            {
                policy.UpdateApprovalStatus("rejected");
            }
            workTask.UpdateRejectionReason(reasonId, reasonDescription);
            workTask.UpdateStatus(WorkTaskStatus.Rejected);
            if (workTask.WorkInstanceId != null && wiById.TryGetValue(workTask.WorkInstanceId.Value, out var workInstance))
            {
                workInstance.UpdateStatus(WorkInstanceStatus.Rejected);
            }
        }

        await CurrentUnitOfWork.SaveChangesAsync();

        var elsaTriggers = new List<(string EventName, string WorkflowInstanceId, string Action)>();
        foreach (var workTask in workTasks.Where(w => w.WorkInstanceId != null))
        {
            if (wiById.TryGetValue(workTask.WorkInstanceId!.Value, out var workInstance))
            {
                var eventName = workTask.EventName ?? workInstance.WorkflowInstanceId;
                elsaTriggers.Add((eventName, workInstance.WorkflowInstanceId, "reject"));
            }
        }

        TriggerElsaBatchInBackground(elsaTriggers);
    }

    private void TriggerElsaBatchInBackground(List<(string EventName, string WorkflowInstanceId, string Action)> triggers)
    {
        if (triggers.Count == 0) return;

        _ = Task.Run(async () =>
        {
            foreach (var (eventName, workflowInstanceId, action) in triggers)
            {
                try
                {
                    await _elsaWorkflowService.TriggerApprovalAsync(eventName, workflowInstanceId, action, null);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Elsa batch trigger failed. EventName={EventName}, WorkflowInstanceId={WorkflowInstanceId}, Action={Action}", eventName, workflowInstanceId, action);
                }
                await Task.Delay(500);
            }
        });
    }

    private async Task EnsurePermissionForBusinessCodeAsync(string businessCode, bool view)
    {
        string permission;
        if (businessCode == TerminatePolicyApprovalBusinessCode)
            permission = view ? PolicyPermissions.TerminateApproval.View : PolicyPermissions.TerminateApproval.Approve;
        else if (businessCode == EndorsementPolicyApprovalBusinessCode)
            permission = view ? PolicyPermissions.EndorsementApproval.View : PolicyPermissions.EndorsementApproval.Approve;
        else
            permission = view ? PolicyPermissions.RequestApproval.View : PolicyPermissions.RequestApproval.Approve;

        if (!await _permissionChecker.IsGrantedAsync(permission))
        {
            throw new Volo.Abp.Authorization.AbpAuthorizationException(permission);
        }
    }

    private static IQueryable<PolicyApprovalListRow> ApplyFiltersOnPair(IQueryable<PolicyApprovalListRow> query, GetPolicyRequestApprovalListInput input)
    {
        var q = query;
        if (!string.IsNullOrWhiteSpace(input.PolicyNo))
        {
            var policyNoPattern = "%" + input.PolicyNo.Trim() + "%";
            q = q.Where(x =>
                EF.Functions.ILike(x.Policy.PolicyNo ?? string.Empty, policyNoPattern)
                || EF.Functions.ILike(x.Policy.InsurerPolicyNo ?? string.Empty, policyNoPattern));
        }
        if (input.ContractId.HasValue)
            q = q.Where(x => x.Policy.ContractId == input.ContractId.Value);
        if (input.LobId.HasValue)
            q = q.Where(x => x.Policy.LobId == input.LobId.Value);
        if (input.PolicyTypeId.HasValue)
            q = q.Where(x => x.Policy.PolicyTypeId == input.PolicyTypeId.Value);
        if (input.PartnerId.HasValue)
            q = q.Where(x => x.Policy.PartnerId == input.PartnerId.Value);
        if (input.SellerId.HasValue)
            q = q.Where(x => x.Policy.SellerId == input.SellerId.Value);
        if (input.CurrencyId.HasValue)
            q = q.Where(x => x.Policy.CurrencyId == input.CurrencyId.Value);
        if (input.SellType.HasValue)
            q = q.Where(x => x.Policy.SellType == input.SellType.Value);
        if (input.Status.HasValue)
            q = q.Where(x => x.Policy.Status == input.Status.Value);
        if (!string.IsNullOrWhiteSpace(input.ApprovalStatus))
        {
            var wtStatus = MapApprovalStatusToWorkTaskStatus(input.ApprovalStatus);
            if (wtStatus.HasValue)
                q = q.Where(x => x.WorkTaskStatus == wtStatus.Value);
        }
        if (input.ChannelId.HasValue)
            q = q.Where(x => x.Policy.ChannelId == input.ChannelId.Value);
        if (input.CustomerId.HasValue)
            q = q.Where(x => x.Policy.ContractId != null && x.Policy.Contract!.CustomerId == input.CustomerId.Value);
        if (!string.IsNullOrWhiteSpace(input.ContractType) && Enum.TryParse<PolicyContractType>(input.ContractType, out var contractType))
            q = q.Where(x => x.Policy.ContractId != null && x.Policy.Contract!.Type == contractType);
        if (!string.IsNullOrWhiteSpace(input.ContractStatus) && Enum.TryParse<PolicyContractStatus>(input.ContractStatus, out var contractStatus))
            q = q.Where(x => x.Policy.ContractId != null && x.Policy.Contract!.Status == contractStatus);
        if (!string.IsNullOrWhiteSpace(input.CertificateNo))
            q = q.Where(x => x.Policy.PolicyVersions.Any(v => v.PolicyCertificates.Any(c => EF.Functions.ILike(c.CertificateNo ?? string.Empty, $"%{input.CertificateNo}%"))));
        if (input.ImplementerId.HasValue || input.PolicyIssuerId.HasValue)
        {
            var implementerId = input.PolicyIssuerId ?? input.ImplementerId;
            q = q.Where(x => x.Policy.ImplementerId == implementerId!.Value);
        }
        if (input.EffectiveDateFrom.HasValue)
            q = q.Where(x => x.Policy.OrgEffectDate >= input.EffectiveDateFrom.Value);
        if (input.EffectiveDateTo.HasValue)
            q = q.Where(x => x.Policy.OrgEffectDate <= input.EffectiveDateTo.Value);
        if (input.ExpiryDateFrom.HasValue)
            q = q.Where(x => x.Policy.OrgExpireDate >= input.ExpiryDateFrom.Value);
        if (input.ExpiryDateTo.HasValue)
            q = q.Where(x => x.Policy.OrgExpireDate <= input.ExpiryDateTo.Value);
        if (!string.IsNullOrWhiteSpace(input.CarPlate))
        {
            var carPlateFilter = input.CarPlate.Trim();
            var carPlateClear = System.Text.RegularExpressions.Regex.Replace(carPlateFilter, "[^a-zA-Z0-9]", string.Empty);
            var carPlatePattern = "%" + EscapeForLike(carPlateFilter) + "%";
            var carPlateClearPattern = "%" + EscapeForLike(carPlateClear) + "%";
            q = q.Where(x => x.Policy.PolicyVersions.Any(v =>
                v.PolicyRiskObjects.Any(ro =>
                    ro.PolicyRiskMotors.Any(m =>
                        EF.Functions.ILike(m.CarPlate ?? string.Empty, carPlatePattern)
                        || EF.Functions.ILike(m.CarPlateClear ?? string.Empty, carPlateClearPattern)))));
        }
        if (!string.IsNullOrWhiteSpace(input.CarVin))
            q = q.Where(x => x.Policy.PolicyVersions.Any(v => v.PolicyRiskObjects.Any(ro => ro.PolicyRiskMotors.Any(m => EF.Functions.ILike(m.CarVin ?? string.Empty, $"%{input.CarVin}%")))));
        if (!string.IsNullOrWhiteSpace(input.CarEngineNumber))
            q = q.Where(x => x.Policy.PolicyVersions.Any(v => v.PolicyRiskObjects.Any(ro => ro.PolicyRiskMotors.Any(m => EF.Functions.ILike(m.CarEngineNumber ?? string.Empty, $"%{input.CarEngineNumber}%")))));
        if (input.PrimaryInsurancePartnerId.HasValue)
            q = q.Where(x => x.Policy.ContractId != null && x.Policy.Contract!.InsurerId == input.PrimaryInsurancePartnerId.Value);
        if (!string.IsNullOrWhiteSpace(input.ImportLotNumber))
            q = q.Where(x => EF.Functions.ILike(x.Policy.LotImportCode ?? string.Empty, $"%{input.ImportLotNumber}%"));
        return q;
    }

    private static string EscapeForLike(string pattern)
    {
        if (string.IsNullOrEmpty(pattern))
            return pattern;
        return pattern
            .Replace("\\", "\\\\")
            .Replace("%", "\\%")
            .Replace("_", "\\_");
    }

    private static IQueryable<PolicyApprovalListRow> ApplySortingOnPair(IQueryable<PolicyApprovalListRow> query, PagedAndSortedResultRequestDto input)
    {
        var sorting = string.IsNullOrWhiteSpace(input.Sorting) ? "-CreationTime" : input.Sorting;
        var isDesc = sorting.StartsWith("-");
        var property = isDesc ? sorting.TrimStart('-') : sorting;
        return property switch
        {
            "PolicyNo" => isDesc ? query.OrderByDescending(x => x.Policy.PolicyNo) : query.OrderBy(x => x.Policy.PolicyNo),
            "OrgEffectDate" => isDesc ? query.OrderByDescending(x => x.Policy.OrgEffectDate) : query.OrderBy(x => x.Policy.OrgEffectDate),
            "OrgExpireDate" => isDesc ? query.OrderByDescending(x => x.Policy.OrgExpireDate) : query.OrderBy(x => x.Policy.OrgExpireDate),
            "CreationTime" => isDesc ? query.OrderByDescending(x => x.WorkTaskCreationTime) : query.OrderBy(x => x.WorkTaskCreationTime),
            "WorkTaskStatus" => query
                .OrderBy(x => x.WorkTaskStatus == WorkTaskStatus.WaitApprove ? 0 : 1)
                .ThenByDescending(x => x.WorkTaskCreationTime),
            _ => query.OrderByDescending(x => x.WorkTaskCreationTime)
        };
    }

    private static WorkTaskStatus? MapApprovalStatusToWorkTaskStatus(string approvalStatus)
    {
        var s = (approvalStatus ?? "").Trim().ToLowerInvariant();
        return s switch
        {
            "pending" => WorkTaskStatus.WaitApprove,
            "approved" => WorkTaskStatus.Approved,
            "rejected" => WorkTaskStatus.Rejected,
            _ => null
        };
    }

    private static IQueryable<PolicyApprovalListRow> ApplyPagingOnPair(IQueryable<PolicyApprovalListRow> query, PagedAndSortedResultRequestDto input)
    {
        return query.Skip(input.SkipCount).Take(input.MaxResultCount);
    }

    private static IQueryable<iOne.Policies.Policy> ApplyFilters(IQueryable<iOne.Policies.Policy> query, GetPolicyRequestApprovalListInput input)
    {
        if (!string.IsNullOrWhiteSpace(input.PolicyNo))
            query = query.Where(x => EF.Functions.ILike(x.PolicyNo, $"%{input.PolicyNo}%"));
        if (input.ContractId.HasValue)
            query = query.Where(x => x.ContractId == input.ContractId.Value);
        if (input.LobId.HasValue)
            query = query.Where(x => x.LobId == input.LobId.Value);
        if (input.PolicyTypeId.HasValue)
            query = query.Where(x => x.PolicyTypeId == input.PolicyTypeId.Value);
        if (input.PartnerId.HasValue)
            query = query.Where(x => x.PartnerId == input.PartnerId.Value);
        if (input.SellerId.HasValue)
            query = query.Where(x => x.SellerId == input.SellerId.Value);
        if (input.CurrencyId.HasValue)
            query = query.Where(x => x.CurrencyId == input.CurrencyId.Value);
        if (input.SellType.HasValue)
            query = query.Where(x => x.SellType == input.SellType.Value);
        if (input.Status.HasValue)
            query = query.Where(x => x.Status == input.Status.Value);
        if (!string.IsNullOrWhiteSpace(input.ApprovalStatus))
            query = query.Where(x => x.ApprovalStatus == input.ApprovalStatus);
        if (input.ChannelId.HasValue)
            query = query.Where(x => x.ChannelId == input.ChannelId.Value);
        if (input.CustomerId.HasValue)
            query = query.Where(x => x.ContractId != null && x.Contract!.CustomerId == input.CustomerId.Value);
        if (!string.IsNullOrWhiteSpace(input.ContractType) && Enum.TryParse<PolicyContractType>(input.ContractType, out var contractType))
            query = query.Where(x => x.ContractId != null && x.Contract!.Type == contractType);
        if (!string.IsNullOrWhiteSpace(input.ContractStatus) && Enum.TryParse<PolicyContractStatus>(input.ContractStatus, out var contractStatus))
            query = query.Where(x => x.ContractId != null && x.Contract!.Status == contractStatus);
        if (!string.IsNullOrWhiteSpace(input.CertificateNo))
            query = query.Where(x => x.PolicyVersions.Any(v => v.PolicyCertificates.Any(c => EF.Functions.ILike(c.CertificateNo ?? string.Empty, $"%{input.CertificateNo}%"))));
        if (input.ImplementerId.HasValue || input.PolicyIssuerId.HasValue)
        {
            var implementerId = input.PolicyIssuerId ?? input.ImplementerId;
            query = query.Where(x => x.ImplementerId == implementerId!.Value);
        }
        if (input.EffectiveDateFrom.HasValue)
            query = query.Where(x => x.OrgEffectDate >= input.EffectiveDateFrom.Value);
        if (input.EffectiveDateTo.HasValue)
            query = query.Where(x => x.OrgEffectDate <= input.EffectiveDateTo.Value);
        if (input.ExpiryDateFrom.HasValue)
            query = query.Where(x => x.OrgExpireDate >= input.ExpiryDateFrom.Value);
        if (input.ExpiryDateTo.HasValue)
            query = query.Where(x => x.OrgExpireDate <= input.ExpiryDateTo.Value);
        if (!string.IsNullOrWhiteSpace(input.CarPlate))
            query = query.Where(x => x.PolicyVersions.Any(v => v.PolicyRiskObjects.Any(ro => ro.PolicyRiskMotors.Any(m => EF.Functions.ILike(m.CarPlate ?? string.Empty, $"%{input.CarPlate}%")))));
        if (!string.IsNullOrWhiteSpace(input.CarVin))
            query = query.Where(x => x.PolicyVersions.Any(v => v.PolicyRiskObjects.Any(ro => ro.PolicyRiskMotors.Any(m => EF.Functions.ILike(m.CarVin ?? string.Empty, $"%{input.CarVin}%")))));
        if (!string.IsNullOrWhiteSpace(input.CarEngineNumber))
            query = query.Where(x => x.PolicyVersions.Any(v => v.PolicyRiskObjects.Any(ro => ro.PolicyRiskMotors.Any(m => EF.Functions.ILike(m.CarEngineNumber ?? string.Empty, $"%{input.CarEngineNumber}%")))));
        if (input.PrimaryInsurancePartnerId.HasValue)
            query = query.Where(x => x.ContractId != null && x.Contract!.InsurerId == input.PrimaryInsurancePartnerId.Value);
        if (!string.IsNullOrWhiteSpace(input.ImportLotNumber))
            query = query.Where(x => EF.Functions.ILike(x.LotImportCode ?? string.Empty, $"%{input.ImportLotNumber}%"));
        return query;
    }

    private static IQueryable<iOne.Policies.Policy> ApplySorting(IQueryable<iOne.Policies.Policy> query, PagedAndSortedResultRequestDto input)
    {
        var sorting = string.IsNullOrWhiteSpace(input.Sorting) ? "PolicyNo" : input.Sorting;
        var isDesc = sorting.StartsWith("-");
        var property = isDesc ? sorting.TrimStart('-') : sorting;
        return property switch
        {
            "PolicyNo" => isDesc ? query.OrderByDescending(x => x.PolicyNo) : query.OrderBy(x => x.PolicyNo),
            "OrgEffectDate" => isDesc ? query.OrderByDescending(x => x.OrgEffectDate) : query.OrderBy(x => x.OrgEffectDate),
            "OrgExpireDate" => isDesc ? query.OrderByDescending(x => x.OrgExpireDate) : query.OrderBy(x => x.OrgExpireDate),
            "CreationTime" => isDesc ? query.OrderByDescending(x => x.CreationTime) : query.OrderBy(x => x.CreationTime),
            _ => query.OrderBy(x => x.PolicyNo)
        };
    }

    private static IQueryable<iOne.Policies.Policy> ApplyPaging(IQueryable<iOne.Policies.Policy> query, PagedAndSortedResultRequestDto input)
    {
        return query.Skip(input.SkipCount).Take(input.MaxResultCount);
    }
}
