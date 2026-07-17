using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using iOne.AdminConfigs;
using iOne.ClaimDocuments;
using iOne.ClaimFolderExposures;
using iOne.ClaimFolderIncidentObjects;
using iOne.ClaimFolderItemPlans;
using iOne.ClaimFolderItems;
using iOne.ClaimFolderQuotationApprovals;
using iOne.ClaimFolders;
using iOne.Claim.Localization;
using iOne.ClaimIncidentRiskMotors;
using iOne.ClaimIncidents;
using iOne.HrDepartments;
using iOne.HrEmployees;
using iOne.Policies;
using iOne.ProCoverages;
using iOne.ProProducts;
using iOne.ResObjectTypes;
using iOne.Workflow;
using iOne.WorkInstances;
using iOne.ResDocuments;
using iOne.ResDocumentTypes;
using iOne.ResClaimPlans;
using iOne.ResObjectTypeItems;
using iOne.ResRisks;
using iOne.WorkTasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace iOne.Claim.Claims;

public class ClaimDetailedAssessmentAppService : ApplicationService, IClaimDetailedAssessmentAppService
{
    private const string ProfileDocumentGroupCode = "CLAIM_ADJUST_ONSITE_PROFILE";
    private const string DetailedProfileMetadataGroupCode = "DETAILED_PROFILE_META";

    protected IRepository<WorkTask, Guid> WorkTaskRepository { get; }
    protected IRepository<ClaimFolder, Guid> ClaimFolderRepository { get; }
    protected IRepository<ClaimIncident, Guid> ClaimIncidentRepository { get; }
    protected IRepository<ClaimIncidentRiskMotor, Guid> ClaimIncidentRiskMotorRepository { get; }
    protected IRepository<ClaimDocument, Guid> ClaimDocumentRepository { get; }
    protected IRepository<ClaimFolderIncidentObject, Guid> ClaimFolderIncidentObjectRepository { get; }
    protected IRepository<ClaimFolderExposure, Guid> ClaimFolderExposureRepository { get; }
    protected IRepository<ClaimFolderItem, Guid> ClaimFolderItemRepository { get; }
    protected IRepository<ClaimFolderItemPlan, Guid> ClaimFolderItemPlanRepository { get; }
    protected IRepository<ClaimFolderQuotationApproval, Guid> QuotationApprovalRepository { get; }
    protected IRepository<HrEmployee, Guid> EmployeeRepository { get; }
    protected IRepository<HrDepartment, Guid> DepartmentRepository { get; }
    protected IRepository<AdminConfig, Guid> AdminConfigRepository { get; }
    protected IRepository<ProProductCoverage, Guid> ProProductCoverageRepository { get; }
    protected IRepository<ProCoverage, Guid> ProCoverageRepository { get; }
    protected IRepository<ResRisk, Guid> ResRiskRepository { get; }
    protected IRepository<ResClaimPlan, Guid> ResClaimPlanRepository { get; }
    protected IRepository<ResObjectTypeItem, Guid> ResObjectTypeItemRepository { get; }
    protected IRepository<ResObjectType, Guid> ResObjectTypeRepository { get; }
    protected IRepository<ResDocument, Guid> ResDocumentRepository { get; }
    protected IRepository<ResDocumentType, Guid> ResDocumentTypeRepository { get; }
    protected IRepository<Policy, Guid> PolicyRepository { get; }
    protected IRepository<PolicyVersion, Guid> PolicyVersionRepository { get; }
    protected IRepository<PolicyProduct, Guid> PolicyProductRepository { get; }
    protected IRepository<PolicyCoverage, Guid> PolicyCoverageRepository { get; }
    private readonly IElsaWorkflowService _elsaWorkflowService;
    private readonly IWorkInstanceRepository _workInstanceRepository;

    public ClaimDetailedAssessmentAppService(
        IRepository<WorkTask, Guid> workTaskRepository,
        IRepository<ClaimFolder, Guid> claimFolderRepository,
        IRepository<ClaimIncident, Guid> claimIncidentRepository,
        IRepository<ClaimIncidentRiskMotor, Guid> claimIncidentRiskMotorRepository,
        IRepository<ClaimDocument, Guid> claimDocumentRepository,
        IRepository<ClaimFolderIncidentObject, Guid> claimFolderIncidentObjectRepository,
        IRepository<ClaimFolderExposure, Guid> claimFolderExposureRepository,
        IRepository<ClaimFolderItem, Guid> claimFolderItemRepository,
        IRepository<ClaimFolderItemPlan, Guid> claimFolderItemPlanRepository,
        IRepository<ClaimFolderQuotationApproval, Guid> quotationApprovalRepository,
        IRepository<HrEmployee, Guid> employeeRepository,
        IRepository<HrDepartment, Guid> departmentRepository,
        IRepository<AdminConfig, Guid> adminConfigRepository,
        IRepository<ProProductCoverage, Guid> proProductCoverageRepository,
        IRepository<ProCoverage, Guid> proCoverageRepository,
        IRepository<ResRisk, Guid> resRiskRepository,
        IRepository<ResClaimPlan, Guid> resClaimPlanRepository,
        IRepository<ResObjectTypeItem, Guid> resObjectTypeItemRepository,
        IRepository<ResObjectType, Guid> resObjectTypeRepository,
        IRepository<ResDocument, Guid> resDocumentRepository,
        IRepository<ResDocumentType, Guid> resDocumentTypeRepository,
        IRepository<Policy, Guid> policyRepository,
        IRepository<PolicyVersion, Guid> policyVersionRepository,
        IRepository<PolicyProduct, Guid> policyProductRepository,
        IRepository<PolicyCoverage, Guid> policyCoverageRepository,
        IElsaWorkflowService elsaWorkflowService,
        IWorkInstanceRepository workInstanceRepository)
    {
        WorkTaskRepository = workTaskRepository;
        ClaimFolderRepository = claimFolderRepository;
        ClaimIncidentRepository = claimIncidentRepository;
        ClaimIncidentRiskMotorRepository = claimIncidentRiskMotorRepository;
        ClaimDocumentRepository = claimDocumentRepository;
        ClaimFolderIncidentObjectRepository = claimFolderIncidentObjectRepository;
        ClaimFolderExposureRepository = claimFolderExposureRepository;
        ClaimFolderItemRepository = claimFolderItemRepository;
        ClaimFolderItemPlanRepository = claimFolderItemPlanRepository;
        QuotationApprovalRepository = quotationApprovalRepository;
        EmployeeRepository = employeeRepository;
        DepartmentRepository = departmentRepository;
        AdminConfigRepository = adminConfigRepository;
        ProProductCoverageRepository = proProductCoverageRepository;
        ProCoverageRepository = proCoverageRepository;
        ResRiskRepository = resRiskRepository;
        ResClaimPlanRepository = resClaimPlanRepository;
        ResObjectTypeItemRepository = resObjectTypeItemRepository;
        ResObjectTypeRepository = resObjectTypeRepository;
        ResDocumentRepository = resDocumentRepository;
        ResDocumentTypeRepository = resDocumentTypeRepository;
        PolicyRepository = policyRepository;
        PolicyVersionRepository = policyVersionRepository;
        PolicyProductRepository = policyProductRepository;
        PolicyCoverageRepository = policyCoverageRepository;
        _elsaWorkflowService = elsaWorkflowService;
        _workInstanceRepository = workInstanceRepository;
        LocalizationResource = typeof(ClaimResource);
    }

    public virtual async Task<DetailedAssessmentDetailDto> GetDetailAsync(Guid workTaskId)
    {
        var currentEmployeeId = await GetCurrentEmployeeIdAsync();
        var workTask = await WorkTaskRepository.GetAsync(workTaskId);
        var assessor = workTask.AssigneeId.HasValue
            ? await EmployeeRepository.FirstOrDefaultAsync(x => x.Id == workTask.AssigneeId.Value)
            : null;

        var assessorName = assessor?.FullName;

        var assessorDeptId = workTask.AssigneeDepartmentId ?? assessor?.DepartmentId;
        var assessorDeptName = assessorDeptId.HasValue
            ? (await DepartmentRepository.FirstOrDefaultAsync(x => x.Id == assessorDeptId.Value))?.Name
            : null;

        var claimFolder = await ClaimFolderRepository.FirstOrDefaultAsync(x => x.Id == workTask.BusinessKey);
        var claimId = claimFolder?.ClaimId ?? await ResolveClaimIdAsync(workTask.BusinessKey);
        var incident = await ClaimIncidentRepository.FirstOrDefaultAsync(x => x.IncidentId == claimId);
        var riskMotor = incident == null
            ? null
            : await ClaimIncidentRiskMotorRepository.FirstOrDefaultAsync(x => x.IncidentObjectId == incident.Id);

        var coverageOptions = await GetClaimFolderExposureCoverageOptionsAsync(workTask.Id);
        if (coverageOptions.Count == 0)
        {
            coverageOptions = await GetCoverageOptionsAsync(workTask.Id, claimFolder);
        }

        return new DetailedAssessmentDetailDto
        {
            WorkTaskId = workTask.Id,
            WorkTaskStatus = workTask.Status,
            ClaimId = claimId,
            IsReporter = workTask.ReporterId == currentEmployeeId,
            AssessorDeptName = assessorDeptName,
            AssessorName = assessorName,
            StartDate = workTask.StartDate,
            EndDate = workTask.EndDate,
            DriverName = riskMotor?.DriverName,
            DriverSex = riskMotor?.DriverSex,
            DriverPhone = riskMotor?.DriverPhone,
            DriverIdNo = riskMotor?.DriverIdNo,
            DriverLicenseNo = riskMotor?.DriverLicenseNo,
            DriverLicenseEffectDate = riskMotor?.DriverLicenseEffectDate,
            DriverLicenseExpireDate = riskMotor?.DriverLicenseExpireDate,
            DriverLicenseLevel = riskMotor?.DriverLicenseLevel,
            CarRegistryNo = riskMotor?.CarRegistryNo,
            CarRegistryEffectDate = riskMotor?.CarRegistryEffectDate,
            CarRegistryExpireDate = riskMotor?.CarRegistryExpireDate,
            LicenseLevelOptions = await GetLicenseLevelOptionsAsync(),
            CoverageOptions = coverageOptions,
            RiskOptions = await GetRiskOptionsAsync(),
            ClaimPlanOptions = await GetClaimPlanOptionsAsync(),
            ItemOptions = await GetItemOptionsAsync(),
            Sections = claimFolder == null
                ? BuildDefaultSections(workTask.StartDate, coverageOptions.FirstOrDefault())
                : await BuildSectionsAsync(claimFolder.Id, workTask.StartDate, coverageOptions),
            DocumentRows = await BuildDocumentRowsAsync(claimFolder?.Id)
        };
    }

    public virtual async Task<List<DetailedAssessmentOptionDto>> GetCoverageOptionsAsync(Guid workTaskId)
    {
        return await GetClaimFolderExposureCoverageOptionsAsync(workTaskId);
    }

    public virtual async Task SaveAsync(Guid workTaskId, SaveDetailedAssessmentInput input)
    {
        var currentEmployeeId = await GetCurrentEmployeeIdAsync();

        var workTask = await WorkTaskRepository.GetAsync(workTaskId);
        if (workTask.BusinessCode != "CLAIM_DETAIL_ASSESSMENT")
            throw new Volo.Abp.UserFriendlyException(L["ClaimTask:NotAuthorized"].Value);

        if (workTask.AssigneeId != currentEmployeeId && workTask.ReporterId != currentEmployeeId)
            throw new Volo.Abp.UserFriendlyException(L["ClaimTask:NotAuthorized"].Value);

        var claimFolder = await ClaimFolderRepository.FirstOrDefaultAsync(x => x.Id == workTask.BusinessKey);
        if (claimFolder == null)
            throw new Volo.Abp.UserFriendlyException("Không tìm thấy hồ sơ bồi thường.");

        if (workTask.Status == WorkTaskStatus.New)
        {
            workTask.UpdateStatus(WorkTaskStatus.InProgress);
            workTask.UpdateActualDates(workTask.ActualStartDate ?? Clock.Now, workTask.ActualEndDate);
            await WorkTaskRepository.UpdateAsync(workTask);
        }
        else if (workTask.Status == WorkTaskStatus.Completed)
        {
            if (!await CanUpdateCompletedDetailedAssessmentAsync(claimFolder.ClaimId, claimFolder.Id))
            {
                throw new Volo.Abp.UserFriendlyException(L["ClaimTask:InvalidStatus"].Value);
            }
        }
        else if (workTask.Status != WorkTaskStatus.InProgress && workTask.Status != WorkTaskStatus.Accepted)
        {
            throw new Volo.Abp.UserFriendlyException(L["ClaimTask:InvalidStatus"].Value);
        }

        var claimId = claimFolder.ClaimId;
        var incident = await ClaimIncidentRepository.FirstOrDefaultAsync(x => x.IncidentId == claimId);
        if (incident == null)
            throw new Volo.Abp.UserFriendlyException("Không tìm thấy thông tin tổn thất của yêu cầu.");

        ValidateInput(input, incident.IncidentDate);

        var riskMotor = await ClaimIncidentRiskMotorRepository.FirstOrDefaultAsync(x => x.IncidentObjectId == incident.Id);
        if (riskMotor == null)
        {
            riskMotor = new ClaimIncidentRiskMotor(GuidGenerator.Create(), incident.Id);
            await ClaimIncidentRiskMotorRepository.InsertAsync(riskMotor, autoSave: true);
        }

        riskMotor.UpdateDriverName(input.DriverName?.Trim());
        riskMotor.UpdateDriverSex(input.DriverSex?.Trim());
        riskMotor.UpdateDriverPhone(input.DriverPhone?.Trim());
        riskMotor.UpdateDriverIdNo(input.DriverIdNo?.Trim());
        riskMotor.UpdateDriverLicenseNo(input.DriverLicenseNo?.Trim());
        riskMotor.UpdateDriverLicenseEffectDate(input.DriverLicenseEffectDate);
        riskMotor.UpdateDriverLicenseExpireDate(input.DriverLicenseExpireDate);
        riskMotor.UpdateDriverLicenseLevel(input.DriverLicenseLevel?.Trim());
        riskMotor.UpdateCarRegistryNo(input.CarRegistryNo?.Trim());
        riskMotor.UpdateCarRegistryEffectDate(input.CarRegistryEffectDate);
        riskMotor.UpdateCarRegistryExpireDate(input.CarRegistryExpireDate);
        await ClaimIncidentRiskMotorRepository.UpdateAsync(riskMotor);

        await ReplaceDetailedAssessmentStructureAsync(claimFolder, incident, riskMotor, input);
        await UpsertDetailedItemDocumentsAsync(claimId, claimFolder.Id, input.Sections);
        await UpsertDetailedDocumentsAsync(claimId, claimFolder.Id, input.DocumentRows);
    }

    public virtual async Task CompleteAsync(Guid workTaskId, SaveDetailedAssessmentInput input)
    {
        await SaveAsync(workTaskId, input);

        var workTask = await WorkTaskRepository.GetAsync(workTaskId);
        if (workTask.Status == WorkTaskStatus.Cancelled || workTask.Status == WorkTaskStatus.Completed)
            throw new Volo.Abp.UserFriendlyException(L["ClaimTask:InvalidStatus"].Value);

        workTask.UpdateStatus(WorkTaskStatus.Completed);
        workTask.UpdateActualDates(workTask.ActualStartDate ?? Clock.Now, Clock.Now);
        await WorkTaskRepository.UpdateAsync(workTask);

        if (workTask.WorkInstanceId != null)
        {
            var workInstance = await _workInstanceRepository.GetAsync(workTask.WorkInstanceId.Value);
            var eventName = workTask.EventName ?? workInstance.WorkflowInstanceId;
            await _elsaWorkflowService.TriggerApprovalAsync(eventName, workInstance.WorkflowInstanceId, "complete", null);
        }
    }

    public virtual async Task AcceptAsync(Guid workTaskId)
    {
        var currentEmployeeId = await GetCurrentEmployeeIdAsync();
        var workTask = await WorkTaskRepository.GetAsync(workTaskId);
        if (workTask.BusinessCode != "CLAIM_DETAIL_ASSESSMENT" || workTask.AssigneeId != currentEmployeeId)
            throw new Volo.Abp.UserFriendlyException(L["ClaimTask:NotAuthorized"].Value);
        if (workTask.Status != WorkTaskStatus.New)
            throw new Volo.Abp.UserFriendlyException(L["ClaimTask:InvalidStatus"].Value);

        workTask.UpdateStatus(WorkTaskStatus.InProgress);
        workTask.UpdateActualDates(Clock.Now, null);
        await WorkTaskRepository.UpdateAsync(workTask);
    }

    public virtual async Task CancelAsync(Guid workTaskId)
    {
        var workTask = await WorkTaskRepository.GetAsync(workTaskId);
        if (workTask.BusinessCode != "CLAIM_DETAIL_ASSESSMENT")
            throw new Volo.Abp.UserFriendlyException(L["ClaimTask:NotAuthorized"].Value);

        if (workTask.Status != WorkTaskStatus.New && workTask.Status != WorkTaskStatus.Rejected)
            throw new Volo.Abp.UserFriendlyException(L["ClaimTask:InvalidStatus"].Value);

        var currentEmployeeId = await GetCurrentEmployeeIdAsync();
        if (workTask.ReporterId != currentEmployeeId)
            throw new Volo.Abp.UserFriendlyException(L["ClaimTask:NotAuthorized"].Value);

        workTask.UpdateStatus(WorkTaskStatus.Cancelled);
        workTask.UpdateActualDates(workTask.ActualStartDate, Clock.Now);
        await WorkTaskRepository.UpdateAsync(workTask);

        var claimFolder = await ClaimFolderRepository.FirstOrDefaultAsync(x => x.Id == workTask.BusinessKey);
        if (claimFolder != null)
        {
            claimFolder.UpdateStatus(ClaimFolderStatus.Cancelled);
            claimFolder.UpdateCancelInfo(Clock.Now, currentEmployeeId, null, null);
            await ClaimFolderRepository.UpdateAsync(claimFolder);
        }

        if (workTask.WorkInstanceId != null)
        {
            var workInstance = await _workInstanceRepository.GetAsync(workTask.WorkInstanceId.Value);
            var eventName = workTask.EventName ?? workInstance.WorkflowInstanceId;
            await _elsaWorkflowService.TriggerApprovalAsync(eventName, workInstance.WorkflowInstanceId, "cancel", null);
        }
    }

    public virtual async Task RejectAsync(Guid workTaskId, RejectClaimTaskInput input)
    {
        var currentEmployeeId = await GetCurrentEmployeeIdAsync();
        var workTask = await WorkTaskRepository.GetAsync(workTaskId);
        if (workTask.BusinessCode != "CLAIM_DETAIL_ASSESSMENT" || workTask.AssigneeId != currentEmployeeId)
            throw new Volo.Abp.UserFriendlyException(L["ClaimTask:NotAuthorized"].Value);
        if (workTask.Status != WorkTaskStatus.New)
            throw new Volo.Abp.UserFriendlyException(L["ClaimTask:InvalidStatus"].Value);

        workTask.UpdateRejectionReason(input.ReasonId, input.ReasonDescription?.Trim());
        workTask.UpdateStatus(WorkTaskStatus.Rejected);
        workTask.UpdateActualDates(workTask.ActualStartDate, Clock.Now);
        await WorkTaskRepository.UpdateAsync(workTask);
    }

    public virtual async Task TransferAsync(Guid workTaskId, TransferClaimTaskInput input)
    {
        var currentEmployeeId = await GetCurrentEmployeeIdAsync();
        var workTask = await WorkTaskRepository.GetAsync(workTaskId);
        if (workTask.BusinessCode != "CLAIM_DETAIL_ASSESSMENT" || workTask.AssigneeId != currentEmployeeId)
            throw new Volo.Abp.UserFriendlyException(L["ClaimTask:NotAuthorized"].Value);
        if (workTask.Status != WorkTaskStatus.New)
            throw new Volo.Abp.UserFriendlyException(L["ClaimTask:InvalidStatus"].Value);
        if (input.AssigneeId == currentEmployeeId)
            throw new Volo.Abp.UserFriendlyException("Người điều chuyển phải khác người đang xử lý.");

        var newAssignee = await EmployeeRepository.GetAsync(input.AssigneeId);

        workTask.UpdateRejectionReason(input.ReasonId, input.ReasonDescription?.Trim());
        workTask.UpdateStatus(WorkTaskStatus.Transfer);
        workTask.UpdateActualDates(workTask.ActualStartDate, Clock.Now);
        await WorkTaskRepository.UpdateAsync(workTask);

        var newTask = new WorkTask(
            GuidGenerator.Create(),
            workTask.WorkInstanceId,
            workTask.BusinessCode,
            workTask.BusinessName,
            workTask.BusinessKey,
            workTask.FormKey,
            Guid.NewGuid().ToString("N"),
            workTask.EventName,
            workTask.Name,
            workTask.Description,
            currentEmployeeId,
            workTask.BusinessAuthorityCode,
            input.AssigneeId,
            newAssignee.DepartmentId,
            newAssignee.OrgId,
            workTask.ResBusinessAssigneeId,
            WorkTaskStatus.New,
            workTask.Priority,
            workTask.TaskCategoryId,
            workTask.StartDate,
            workTask.EndDate,
            actualStartDate: null,
            actualEndDate: null,
            reasonId: null,
            reasonDescription: null);
        await WorkTaskRepository.InsertAsync(newTask, autoSave: true);
    }

    public virtual async Task ReassignAsync(Guid workTaskId, ReassignDetailedAssessmentInput input)
    {
        var currentEmployeeId = await GetCurrentEmployeeIdAsync();
        var workTask = await WorkTaskRepository.GetAsync(workTaskId);
        if (workTask.BusinessCode != "CLAIM_DETAIL_ASSESSMENT" || workTask.ReporterId != currentEmployeeId)
            throw new Volo.Abp.UserFriendlyException(L["ClaimTask:NotAuthorized"].Value);

        if (workTask.Status != WorkTaskStatus.Rejected
            && workTask.Status != WorkTaskStatus.Return)
            throw new Volo.Abp.UserFriendlyException(L["ClaimTask:InvalidStatus"].Value);

        var workTaskQuery = await WorkTaskRepository.GetQueryableAsync();
        var hasBlockedTask = await AsyncExecuter.AnyAsync(
            workTaskQuery.Where(wt =>
                wt.BusinessKey == workTask.BusinessKey
                && wt.Id != workTask.Id
                && wt.BusinessCode == "CLAIM_DETAIL_ASSESSMENT"
                && wt.Status != WorkTaskStatus.Rejected
                && wt.Status != WorkTaskStatus.Return));
        if (hasBlockedTask)
            throw new Volo.Abp.UserFriendlyException("Hồ sơ còn công việc giám định chi tiết chưa ở trạng thái từ chối hoặc trả lại, không thể giao lại.");

        var assignee = await EmployeeRepository.FirstOrDefaultAsync(x => x.Id == input.AssigneeId);
        if (assignee == null)
            throw new Volo.Abp.UserFriendlyException("Người giám định không hợp lệ.");
        if (assignee.DepartmentId != input.AssigneeOrganizationId)
            throw new Volo.Abp.UserFriendlyException("Người giám định không thuộc đơn vị đã chọn.");

        await _elsaWorkflowService.InitCreateClaimFolderWorkflowAsync(workTask.BusinessKey, input.AssigneeId.ToString());
    }

    private async Task<List<DetailedAssessmentOptionDto>> GetLicenseLevelOptionsAsync()
    {
        var query = await AdminConfigRepository.GetQueryableAsync();
        var items = await AsyncExecuter.ToListAsync(
            query.Where(x =>
                    (x.Code == "CAR_DRIVER_LICENSE_LEVEL" || x.Code == "DRIVER_LICENSE_LEVEL"))
                .OrderBy(x => x.SubCode));

        if (items.Count == 0)
        {
            return new List<DetailedAssessmentOptionDto>
            {
                new() { Id = "B1", Code = "B1", Name = "B1" },
                new() { Id = "B2", Code = "B2", Name = "B2" },
                new() { Id = "C1", Code = "C1", Name = "C1" },
                new() { Id = "C", Code = "C", Name = "C" }
            };
        }

        return items
            .Select(x => new DetailedAssessmentOptionDto
            {
                Id = x.SubCode,
                Code = x.SubCode,
                Name = x.Name
            })
            .ToList();
    }

    private async Task<List<DetailedAssessmentOptionDto>> GetRiskOptionsAsync()
    {
        var query = await ResRiskRepository.GetQueryableAsync();
        var items = await AsyncExecuter.ToListAsync(
            query.Where(x => x.Status == ResRiskStatus.Active)
                .OrderBy(x => x.Name)
                .Take(200));
        var objectTypeIds = items.Select(x => x.ObjectTypeId).Distinct().ToList();
        var objectTypeById = objectTypeIds.Count == 0
            ? new Dictionary<Guid, ResObjectType>()
            : (await AsyncExecuter.ToListAsync(
                (await ResObjectTypeRepository.GetQueryableAsync()).Where(x => objectTypeIds.Contains(x.Id))))
            .ToDictionary(x => x.Id, x => x);
        return items.Select(x => new DetailedAssessmentOptionDto
        {
            Id = x.Id.ToString(),
            Code = x.Code,
            Name = x.Name,
            ObjectTypeId = x.ObjectTypeId.ToString(),
            ObjectTypeCode = objectTypeById.TryGetValue(x.ObjectTypeId, out var objectType) ? objectType.Code : null,
            ObjectTypeName = objectTypeById.TryGetValue(x.ObjectTypeId, out objectType) ? objectType.Name : null,
            ObjectTypeGroup = objectTypeById.TryGetValue(x.ObjectTypeId, out objectType) ? objectType.ObjectGroup : null,
            ObjectKind = objectTypeById.TryGetValue(x.ObjectTypeId, out objectType)
                ? GetObjectKind(objectType.Code, objectType.Name, objectType.ObjectGroup)
                : null
        }).ToList();
    }

    private async Task<List<DetailedAssessmentOptionDto>> GetClaimPlanOptionsAsync()
    {
        var query = await ResClaimPlanRepository.GetQueryableAsync();
        var items = await AsyncExecuter.ToListAsync(
            query.Where(x => x.Status == ResClaimPlanStatus.Active)
                .OrderBy(x => x.Name)
                .Take(50));
        return items.Select(x => new DetailedAssessmentOptionDto
        {
            Id = x.Id.ToString(),
            Code = x.Code,
            Name = x.Name
        }).ToList();
    }

    private async Task<List<DetailedAssessmentOptionDto>> GetItemOptionsAsync()
    {
        var query = await ResObjectTypeItemRepository.GetQueryableAsync();
        var items = await AsyncExecuter.ToListAsync(
            query.Where(x => x.Status == ResObjectTypeItemStatus.Active)
                .OrderBy(x => x.Name)
                .Take(500));
        var objectTypeIds = items.Select(x => x.ObjectTypeId).Distinct().ToList();
        var objectTypeById = objectTypeIds.Count == 0
            ? new Dictionary<Guid, ResObjectType>()
            : (await AsyncExecuter.ToListAsync(
                (await ResObjectTypeRepository.GetQueryableAsync()).Where(x => objectTypeIds.Contains(x.Id))))
            .ToDictionary(x => x.Id, x => x);
        return items.Select(x => new DetailedAssessmentOptionDto
        {
            Id = x.Id.ToString(),
            Code = x.Code,
            Name = x.Name,
            ObjectTypeId = x.ObjectTypeId.ToString(),
            ObjectTypeCode = objectTypeById.TryGetValue(x.ObjectTypeId, out var objectType) ? objectType.Code : null,
            ObjectTypeName = objectTypeById.TryGetValue(x.ObjectTypeId, out objectType) ? objectType.Name : null,
            ObjectTypeGroup = objectTypeById.TryGetValue(x.ObjectTypeId, out objectType) ? objectType.ObjectGroup : null,
            ObjectKind = objectTypeById.TryGetValue(x.ObjectTypeId, out objectType)
                ? GetObjectKind(objectType.Code, objectType.Name, objectType.ObjectGroup)
                : null
        }).ToList();
    }

    private async Task<List<DetailedAssessmentOptionDto>> GetCoverageOptionsAsync(Guid workTaskId, ClaimFolder? claimFolder)
    {
        if (claimFolder == null)
        {
            Logger.LogInformation(
                "[DetailedAssessment][CoverageOptions] WorkTaskId={WorkTaskId}: claimFolder=null, fallback coverage options.",
                workTaskId);
            return GetFallbackCoverageOptions();
        }

        // Ưu tiên lấy theo dữ liệu thực tế của hồ sơ/tác vụ: coverage trên exposure + object type từ incident object.
        var exposureQuery = await ClaimFolderExposureRepository.GetQueryableAsync();
        var exposures = await AsyncExecuter.ToListAsync(
            exposureQuery.Where(x => x.ClaimFolderId == claimFolder.Id));

        if (exposures.Count > 0)
        {
            var coverageCodes = exposures
                .Select(x => x.CoverageCode)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            var coverageByCode = coverageCodes.Count == 0
                ? new Dictionary<string, ProCoverage>(StringComparer.OrdinalIgnoreCase)
                : (await AsyncExecuter.ToListAsync(
                    (await ProCoverageRepository.GetQueryableAsync()).Where(x => coverageCodes.Contains(x.Code))))
                .ToDictionary(x => x.Code, x => x, StringComparer.OrdinalIgnoreCase);

            var incidentObjectIds = exposures
                .Select(x => x.ClaimFolderIncidentObjectId)
                .Distinct()
                .ToList();
            var incidentObjectById = incidentObjectIds.Count == 0
                ? new Dictionary<Guid, ClaimFolderIncidentObject>()
                : (await AsyncExecuter.ToListAsync(
                    (await ClaimFolderIncidentObjectRepository.GetQueryableAsync()).Where(x => incidentObjectIds.Contains(x.Id))))
                .ToDictionary(x => x.Id, x => x);

            var exposureObjectTypeIds = incidentObjectById.Values
                .Select(x => x.ObjectTypeId)
                .Distinct()
                .ToList();
            var exposureObjectTypeById = exposureObjectTypeIds.Count == 0
                ? new Dictionary<Guid, ResObjectType>()
                : (await AsyncExecuter.ToListAsync(
                    (await ResObjectTypeRepository.GetQueryableAsync()).Where(x => exposureObjectTypeIds.Contains(x.Id))))
                .ToDictionary(x => x.Id, x => x);

            var coverageOptions = exposures
                .Select(exposure =>
                {
                    incidentObjectById.TryGetValue(exposure.ClaimFolderIncidentObjectId, out var incidentObject);
                    coverageByCode.TryGetValue(exposure.CoverageCode, out var coverage);
                    var objectType = incidentObject != null && exposureObjectTypeById.TryGetValue(incidentObject.ObjectTypeId, out var matchedObjectType)
                        ? matchedObjectType
                        : null;

                    return new DetailedAssessmentOptionDto
                    {
                        Id = coverage?.Id.ToString() ?? GetCoverageIdByCode(exposure.CoverageCode),
                        Code = coverage?.Code ?? exposure.CoverageCode,
                        Name = coverage?.Name ?? GetCoverageNameByCode(exposure.CoverageCode),
                        ObjectTypeId = incidentObject?.ObjectTypeId.ToString(),
                        ObjectTypeCode = objectType?.Code,
                        ObjectTypeName = objectType?.Name,
                        ObjectTypeGroup = objectType?.ObjectGroup,
                        ObjectKind = GetObjectKind(objectType?.Code, objectType?.Name, objectType?.ObjectGroup),
                        Type = coverage != null ? coverage.Type.ToString().ToLowerInvariant() : null
                    };
                })
                .OrderBy(x => x.Name)
                .GroupBy(x => new
                {
                    CoverageId = x.Id,
                    CoverageCode = (x.Code ?? string.Empty).ToUpperInvariant()
                })
                .Select(group => group.First())
                .ToList();

            if (coverageOptions.Count > 0)
            {
                Logger.LogInformation(
                    "[DetailedAssessment][CoverageOptions] WorkTaskId={WorkTaskId}, ClaimFolderId={ClaimFolderId}, Source=Exposure, Count={Count}, Items={Items}",
                    workTaskId,
                    claimFolder.Id,
                    coverageOptions.Count,
                    string.Join(" | ", coverageOptions.Select(x => $"{x.Code}-{x.Name}-{x.ObjectTypeCode}")));
                return coverageOptions;
            }
        }

        if (claimFolder.ProductId == null)
        {
            Logger.LogInformation(
                "[DetailedAssessment][CoverageOptions] WorkTaskId={WorkTaskId}, ClaimFolderId={ClaimFolderId}, ProductId=null, fallback coverage options.",
                workTaskId,
                claimFolder.Id);
            return GetFallbackCoverageOptions();
        }

        var productCoverageQuery = await ProProductCoverageRepository.GetQueryableAsync();
        var productCoverages = await AsyncExecuter.ToListAsync(
            productCoverageQuery.Where(x => x.ProductId == claimFolder.ProductId.Value)
                .OrderBy(x => x.SeqNumber)
                .ThenBy(x => x.CreationTime));
        if (productCoverages.Count == 0)
        {
            Logger.LogInformation(
                "[DetailedAssessment][CoverageOptions] WorkTaskId={WorkTaskId}, ClaimFolderId={ClaimFolderId}, ProductId={ProductId}, product coverages empty, fallback coverage options.",
                workTaskId,
                claimFolder.Id,
                claimFolder.ProductId);
            return GetFallbackCoverageOptions();
        }

        var coverageIds = productCoverages.Select(x => x.CoverageId).Distinct().ToList();
        var coverages = await AsyncExecuter.ToListAsync(
            (await ProCoverageRepository.GetQueryableAsync()).Where(x => coverageIds.Contains(x.Id)));
        var coverageById = coverages.ToDictionary(x => x.Id, x => x);
        var objectTypeIds = coverages.Where(x => x.ObjectTypeId.HasValue).Select(x => x.ObjectTypeId!.Value).Distinct().ToList();
        var objectTypeById = objectTypeIds.Count == 0
            ? new Dictionary<Guid, ResObjectType>()
            : (await AsyncExecuter.ToListAsync(
                (await ResObjectTypeRepository.GetQueryableAsync()).Where(x => objectTypeIds.Contains(x.Id))))
            .ToDictionary(x => x.Id, x => x);

        var productOptions = productCoverages
            .Where(x => coverageById.ContainsKey(x.CoverageId))
            .Select(x =>
            {
                var coverage = coverageById[x.CoverageId];
                var objectType = coverage.ObjectTypeId.HasValue && objectTypeById.TryGetValue(coverage.ObjectTypeId.Value, out var matchedObjectType)
                    ? matchedObjectType
                    : null;
                return new DetailedAssessmentOptionDto
                {
                    Id = coverage.Id.ToString(),
                    Code = coverage.Code,
                    Name = coverage.Name,
                    ObjectTypeId = coverage.ObjectTypeId?.ToString(),
                    ObjectTypeCode = objectType?.Code,
                    ObjectTypeName = objectType?.Name,
                    ObjectTypeGroup = objectType?.ObjectGroup,
                    ObjectKind = GetObjectKind(objectType?.Code, objectType?.Name, objectType?.ObjectGroup),
                    Type = coverage.Type.ToString().ToLowerInvariant()
                };
            })
            .ToList();

        Logger.LogInformation(
            "[DetailedAssessment][CoverageOptions] WorkTaskId={WorkTaskId}, ClaimFolderId={ClaimFolderId}, Source=Product, Count={Count}, Items={Items}",
            workTaskId,
            claimFolder.Id,
            productOptions.Count,
            string.Join(" | ", productOptions.Select(x => $"{x.Code}-{x.Name}-{x.ObjectTypeCode}")));

        return productOptions;
    }

    private async Task<List<DetailedAssessmentOptionDto>> GetClaimFolderExposureCoverageOptionsAsync(Guid workTaskId)
    {
        List<CoverageOptionRawRow> rows;
        using (DataFilter.Disable<ISoftDelete>())
        {
            var workTaskQuery = await WorkTaskRepository.GetQueryableAsync();
            var claimFolderQuery = await ClaimFolderRepository.GetQueryableAsync();
            var claimIncidentQuery = await ClaimIncidentRepository.GetQueryableAsync();
            var exposureQuery = await ClaimFolderExposureRepository.GetQueryableAsync();
            var incidentObjectQuery = await ClaimFolderIncidentObjectRepository.GetQueryableAsync();
            var policyQuery = await PolicyRepository.GetQueryableAsync();
            var policyVersionQuery = await PolicyVersionRepository.GetQueryableAsync();
            var policyProductQuery = await PolicyProductRepository.GetQueryableAsync();
            var policyCoverageQuery = await PolicyCoverageRepository.GetQueryableAsync();
            var productCoverageQuery = await ProProductCoverageRepository.GetQueryableAsync();
            var coverageQuery = await ProCoverageRepository.GetQueryableAsync();
            var objectTypeQuery = await ResObjectTypeRepository.GetQueryableAsync();

            var query =
                from wt in workTaskQuery
                join cf in claimFolderQuery on wt.BusinessKey equals cf.Id
                join ci in claimIncidentQuery on cf.IncidentId equals (Guid?)ci.Id
                join cfe in exposureQuery on cf.Id equals cfe.ClaimFolderId
                join cfioJoin in incidentObjectQuery on cfe.ClaimFolderIncidentObjectId equals cfioJoin.Id into incidentObjects
                from cfio in incidentObjects.DefaultIfEmpty()
                join p in policyQuery on cf.PolicyNo equals p.PolicyNo
                join pv in policyVersionQuery on p.Id equals pv.PolicyId
                join pp in policyProductQuery on new
                    {
                        PolicyVersionId = pv.Id,
                        ProductId = cf.ProductId
                    }
                    equals new
                    {
                        pp.PolicyVersionId,
                        ProductId = (Guid?)pp.ProductId
                    }
                join pc in policyCoverageQuery on pp.Id equals pc.PolicyProductId
                join ppc in productCoverageQuery on new
                    {
                        pc.CoverageId,
                        ProductId = pp.ProductId
                    }
                    equals new
                    {
                        ppc.CoverageId,
                        ppc.ProductId
                    }
                join c in coverageQuery on pc.CoverageId equals c.Id
                join otJoin in objectTypeQuery on cfio.ObjectTypeId equals otJoin.Id into objectTypes
                from ot in objectTypes.DefaultIfEmpty()
                where wt.Id == workTaskId
                    && cfio != null
                    && c.ObjectTypeId == cfio.ObjectTypeId
                    && pv.Status == "active"
                    && ci.IncidentDate.HasValue
                    && ci.IncidentDate.Value >= pv.EffectDate
                    && ci.IncidentDate.Value <= pv.ExpireDate
                select new CoverageOptionRawRow
                {
                    WorkTaskBusinessKey = wt.BusinessKey,
                    ClaimFolderId = cf.Id,
                    ExposureId = cfe.Id,
                    IncidentObjectId = cfio != null ? cfio.Id : (Guid?)null,
                    CoverageId = c.Id,
                    CoverageCode = c.Code,
                    CoverageName = c.Name,
                    CoverageType = c.Type,
                    CoverageObjectTypeId = c.ObjectTypeId,
                    IncidentObjectTypeId = cfio != null ? cfio.ObjectTypeId : (Guid?)null,
                    ObjectTypeCode = ot != null ? ot.Code : null,
                    ObjectTypeName = ot != null ? ot.Name : null,
                    ObjectTypeGroup = ot != null ? ot.ObjectGroup : null
                };

            rows = await AsyncExecuter.ToListAsync(query);
        }

        Logger.LogInformation(
            "[DetailedAssessment][CoverageOptionsApi][Raw] WorkTaskId={WorkTaskId}, RowCount={RowCount}, Rows={Rows}",
            workTaskId,
            rows.Count,
            string.Join(" | ", rows.Select(x =>
                $"BusinessKey={x.WorkTaskBusinessKey};ClaimFolderId={x.ClaimFolderId};ExposureId={x.ExposureId};IncidentObjectId={x.IncidentObjectId};Coverage={x.CoverageCode}-{x.CoverageName};CoverageObjectTypeId={x.CoverageObjectTypeId};IncidentObjectTypeId={x.IncidentObjectTypeId};ObjectType={x.ObjectTypeCode}-{x.ObjectTypeName}-{x.ObjectTypeGroup}")));

        var options = rows
            .GroupBy(x => new
            {
                x.CoverageId,
                x.CoverageCode,
                x.CoverageName,
                x.CoverageType,
                x.CoverageObjectTypeId,
                x.IncidentObjectTypeId,
                x.ObjectTypeCode,
                x.ObjectTypeName,
                x.ObjectTypeGroup
            })
            .Select(group =>
            {
                var x = group.First();
                return new
                {
                    Option = new DetailedAssessmentOptionDto
                    {
                        Id = x.CoverageId.ToString(),
                        Code = x.CoverageCode,
                        Name = x.CoverageName,
                        ObjectTypeId = x.IncidentObjectTypeId?.ToString(),
                        ObjectTypeCode = x.ObjectTypeCode,
                        ObjectTypeName = x.ObjectTypeName,
                        ObjectTypeGroup = x.ObjectTypeGroup,
                        ObjectKind = GetObjectKind(x.ObjectTypeCode, x.ObjectTypeName, x.ObjectTypeGroup),
                        Type = x.CoverageType.ToString().ToLowerInvariant()
                    },
                    x.CoverageType,
                    x.CoverageCode,
                    x.IncidentObjectTypeId
                };
            })
            .OrderBy(x => x.IncidentObjectTypeId)
            .ThenBy(x => x.CoverageType == ProCoverageTermType.Main ? 0 : 1)
            .ThenBy(x => x.CoverageCode)
            .Select(x => x.Option)
            .ToList();

        Logger.LogInformation(
            "[DetailedAssessment][CoverageOptionsApi] WorkTaskId={WorkTaskId}, Count={Count}, Items={Items}",
            workTaskId,
            options.Count,
            string.Join(" | ", options.Select(x => $"{x.Code}-{x.Name}-{x.ObjectTypeCode}")));

        return options;
    }

    private static List<DetailedAssessmentOptionDto> GetFallbackCoverageOptions()
    {
        return new List<DetailedAssessmentOptionDto>
        {
            new() { Id = "coverage-physical", Code = "VCX", Name = "Vật chất xe", ObjectKind = "vehicle", Type = "main" },
            new() { Id = "coverage-third-party-property", Code = "TNDS_TS", Name = "TNDS BB về tài sản", ObjectKind = "asset", Type = "addon" },
            new() { Id = "coverage-cargo", Code = "HH_TREN_XE", Name = "TNDS về hàng hóa trên xe", ObjectKind = "asset", Type = "addon" }
        };
    }

    private sealed class CoverageOptionRawRow
    {
        public Guid WorkTaskBusinessKey { get; set; }
        public Guid ClaimFolderId { get; set; }
        public Guid ExposureId { get; set; }
        public Guid? IncidentObjectId { get; set; }
        public Guid CoverageId { get; set; }
        public string CoverageCode { get; set; } = string.Empty;
        public string CoverageName { get; set; } = string.Empty;
        public ProCoverageTermType CoverageType { get; set; }
        public Guid? CoverageObjectTypeId { get; set; }
        public Guid? IncidentObjectTypeId { get; set; }
        public string? ObjectTypeCode { get; set; }
        public string? ObjectTypeName { get; set; }
        public string? ObjectTypeGroup { get; set; }
    }

    private static List<DetailedAssessmentSectionDto> BuildDefaultSections(DateTime? startDate, DetailedAssessmentOptionDto? defaultCoverage)
    {
        var issueDate = startDate?.Date ?? DateTime.Today;
        var coverage = defaultCoverage ?? GetFallbackCoverageOptions().First();

        return new List<DetailedAssessmentSectionDto>
        {
            new()
            {
                Id = "section-1",
                CoverageId = coverage.Id,
                CoverageName = coverage.Name,
                ObjectTypeId = coverage.ObjectTypeId,
                ObjectTypeCode = coverage.ObjectTypeCode,
                ObjectTypeName = coverage.ObjectTypeName,
                ObjectTypeGroup = coverage.ObjectTypeGroup,
                ObjectKind = coverage.ObjectKind ?? GetObjectKind(coverage.ObjectTypeCode, coverage.ObjectTypeName, coverage.ObjectTypeGroup),
                Items = new List<DetailedAssessmentItemDto>
                {
                    new()
                    {
                        Id = "item-1",
                        Quantity = 1,
                        IsRecovery = false,
                        IssueDate = issueDate
                    }
                }
            }
        };
    }

    private async Task<List<DetailedAssessmentDocumentRowDto>> BuildConfiguredDocumentRowsAsync()
    {
        var documentTypes = await AsyncExecuter.ToListAsync(
            (await ResDocumentTypeRepository.GetQueryableAsync())
            .Where(x => x.DocumentGroupCode != null && x.DocumentGroupCode == ProfileDocumentGroupCode)
            .OrderBy(x => x.Name));

        return documentTypes.Select((docType, index) => new DetailedAssessmentDocumentRowDto
        {
            Id = $"doc-{index + 1}",
            Code = docType.Code,
            DocumentTypeId = docType.Id.ToString(),
            Name = docType.Name ?? docType.Code ?? docType.Id.ToString()
        }).ToList();
    }

    private static void ValidateInput(SaveDetailedAssessmentInput input, DateTime? incidentDate)
    {
        foreach (var section in input.Sections)
        {
            var objectKind = GetObjectKind(section.ObjectTypeCode, section.ObjectTypeName, section.ObjectTypeGroup);
            foreach (var item in section.Items)
            {
                if (item.Quantity.HasValue && item.Quantity.Value <= 0)
                {
                    throw new Volo.Abp.UserFriendlyException("Số lượng phải lớn hơn 0.");
                }

                if (objectKind == "person"
                    && item.DischargeDate.HasValue
                    && incidentDate.HasValue
                    && item.DischargeDate.Value.Date < incidentDate.Value.Date)
                {
                    throw new Volo.Abp.UserFriendlyException("Ngày ra viện phải lớn hơn hoặc bằng ngày xảy ra tai nạn.");
                }
            }
        }
    }

    private async Task<List<DetailedAssessmentSectionDto>> BuildSectionsAsync(
        Guid claimFolderId,
        DateTime? startDate,
        List<DetailedAssessmentOptionDto> coverageOptions)
    {
        var exposureQuery = await ClaimFolderExposureRepository.GetQueryableAsync();
        var exposures = await AsyncExecuter.ToListAsync(
            exposureQuery.Where(x => x.ClaimFolderId == claimFolderId).OrderBy(x => x.CreationTime));
        if (exposures.Count == 0)
        {
            return BuildDefaultSections(startDate, coverageOptions.FirstOrDefault());
        }

        var itemQuery = await ClaimFolderItemRepository.GetQueryableAsync();
        var items = await AsyncExecuter.ToListAsync(
            itemQuery.Where(x => x.ClaimFolderId == claimFolderId).OrderBy(x => x.CreationTime));
        var claimFolderItemIds = items.Select(x => x.Id).ToList();
        var claimDocuments = items.Count == 0
            ? new List<ClaimDocument>()
            : await AsyncExecuter.ToListAsync(
                (await ClaimDocumentRepository.GetQueryableAsync())
                .Where(x => x.ClaimFolderId == claimFolderId && x.ClaimFolderItemId.HasValue && claimFolderItemIds.Contains(x.ClaimFolderItemId!.Value))
                .OrderBy(x => x.CreationTime));
        var attachmentDocumentIds = claimDocuments
            .Where(x => x.DocumentId.HasValue)
            .Select(x => x.DocumentId!.Value)
            .Distinct()
            .ToList();
        var attachmentResDocs = attachmentDocumentIds.Count == 0
            ? new List<ResDocument>()
            : await AsyncExecuter.ToListAsync(
                (await ResDocumentRepository.GetQueryableAsync()).Where(x => attachmentDocumentIds.Contains(x.Id)));
        var attachmentResDocById = attachmentResDocs.ToDictionary(x => x.Id, x => x);
        var attachmentsByItemId = claimDocuments
            .Where(x => x.ClaimFolderItemId.HasValue && x.DocumentId.HasValue && attachmentResDocById.ContainsKey(x.DocumentId.Value))
            .GroupBy(x => x.ClaimFolderItemId!.Value)
            .ToDictionary(
                group => group.Key,
                group => group.Select(claimDoc =>
                {
                    var resDoc = attachmentResDocById[claimDoc.DocumentId!.Value];
                    return new DetailedAssessmentAttachmentDto
                    {
                        Id = claimDoc.Id.ToString(),
                        ClaimDocumentId = claimDoc.Id.ToString(),
                        DocumentId = claimDoc.DocumentId?.ToString(),
                        FileName = resDoc.FileName ?? "Tệp đính kèm",
                        Url = resDoc.Url
                    };
                }).ToList());
        var itemIds = items
            .Where(x => x.ItemId.HasValue)
            .Select(x => x.ItemId!.Value)
            .Distinct()
            .ToList();
        var itemNameById = itemIds.Count == 0
            ? new Dictionary<Guid, string?>()
            : (await AsyncExecuter.ToListAsync(
                (await ResObjectTypeItemRepository.GetQueryableAsync()).Where(x => itemIds.Contains(x.Id))))
            .ToDictionary(x => x.Id, x => (string?)x.Name);

        var incidentObjectIds = exposures.Select(x => x.ClaimFolderIncidentObjectId).Distinct().ToList();
        var incidentObjectById = incidentObjectIds.Count == 0
            ? new Dictionary<Guid, ClaimFolderIncidentObject>()
            : (await AsyncExecuter.ToListAsync(
                (await ClaimFolderIncidentObjectRepository.GetQueryableAsync()).Where(x => incidentObjectIds.Contains(x.Id))))
            .ToDictionary(x => x.Id, x => x);
        var coverageCodes = exposures.Select(x => x.CoverageCode).Distinct().ToList();
        var coverageByCode = coverageCodes.Count == 0
            ? new Dictionary<string, ProCoverage>(StringComparer.OrdinalIgnoreCase)
            : (await AsyncExecuter.ToListAsync(
                (await ProCoverageRepository.GetQueryableAsync()).Where(x => coverageCodes.Contains(x.Code))))
            .ToDictionary(x => x.Code, x => x, StringComparer.OrdinalIgnoreCase);
        var objectTypeIds = incidentObjectById.Values.Select(x => x.ObjectTypeId).Distinct().ToList();
        var objectTypeById = objectTypeIds.Count == 0
            ? new Dictionary<Guid, ResObjectType>()
            : (await AsyncExecuter.ToListAsync(
                (await ResObjectTypeRepository.GetQueryableAsync()).Where(x => objectTypeIds.Contains(x.Id))))
            .ToDictionary(x => x.Id, x => x);

        return exposures
            .GroupBy(exposure =>
            {
                incidentObjectById.TryGetValue(exposure.ClaimFolderIncidentObjectId, out var incidentObject);
                var objectTypeId = incidentObject?.ObjectTypeId;
                var objectType = objectTypeId.HasValue && objectTypeById.TryGetValue(objectTypeId.Value, out var matchedObjectType)
                    ? matchedObjectType
                    : null;
                var objectKind = GetObjectKind(objectType?.Code, objectType?.Name, objectType?.ObjectGroup);
                return $"{exposure.CoverageCode}|{objectTypeId}|{objectKind}";
            })
            .Select(group =>
            {
                var firstExposure = group.First();
                incidentObjectById.TryGetValue(firstExposure.ClaimFolderIncidentObjectId, out var firstIncidentObject);
                coverageByCode.TryGetValue(firstExposure.CoverageCode, out var coverage);
                var objectType = firstIncidentObject != null && objectTypeById.TryGetValue(firstIncidentObject.ObjectTypeId, out var matchedObjectType)
                    ? matchedObjectType
                    : null;
                var sectionObjectKind = GetObjectKind(objectType?.Code, objectType?.Name, objectType?.ObjectGroup);

                return new DetailedAssessmentSectionDto
                {
                    Id = firstExposure.Id.ToString(),
                    CoverageId = coverage?.Id.ToString() ?? GetCoverageIdByCode(firstExposure.CoverageCode),
                    CoverageName = coverage?.Name ?? GetCoverageNameByCode(firstExposure.CoverageCode),
                    ObjectTypeId = firstIncidentObject?.ObjectTypeId.ToString(),
                    ObjectTypeCode = objectType?.Code,
                    ObjectTypeName = objectType?.Name,
                    ObjectTypeGroup = objectType?.ObjectGroup,
                    ObjectKind = sectionObjectKind,
                    Items = group
                        .SelectMany(exposure => items.Where(x => x.ClaimFolderExposureId == exposure.Id)
                            .Select(x =>
                            {
                                incidentObjectById.TryGetValue(exposure.ClaimFolderIncidentObjectId, out var incidentObject);
                                var personPayload = ParsePersonPayload(x.Description);
                                return new DetailedAssessmentItemDto
                                {
                                    Id = x.Id.ToString(),
                                    ItemId = x.ItemId?.ToString(),
                                    ItemName = x.ItemId.HasValue && itemNameById.TryGetValue(x.ItemId.Value, out var itemName) ? itemName : null,
                                    PersonName = incidentObject?.Name,
                                    PersonIdNo = incidentObject?.IdNo,
                                    Description = personPayload.Description ?? x.Description,
                                    Quantity = x.Quantity,
                                    RiskId = x.RiskId?.ToString(),
                                    ClaimPlanId = sectionObjectKind == "person"
                                        ? FormatCoveragePercent(x.CoveragePercent)
                                        : x.ClaimPlanId?.ToString(),
                                    IsGenuine = x.IsGenuien,
                                    IsRecovery = string.Equals(x.IsRecovery, "Y", StringComparison.OrdinalIgnoreCase),
                                    DischargeDate = incidentObject?.ExitDate ?? personPayload.DischargeDate,
                                    IssueDate = x.IssueDate,
                                    Attachments = attachmentsByItemId.TryGetValue(x.Id, out var attachments)
                                        ? attachments
                                        : new List<DetailedAssessmentAttachmentDto>()
                                };
                            }))
                        .ToList()
                };
            })
            .ToList();
    }

    private async Task<Guid> ResolveClaimIdAsync(Guid businessKey)
    {
        var claimFolder = await ClaimFolderRepository.FirstOrDefaultAsync(x => x.Id == businessKey);
        return claimFolder?.ClaimId ?? businessKey;
    }

    private async Task<bool> CanUpdateCompletedDetailedAssessmentAsync(Guid claimId, Guid claimFolderId)
    {
        var latestQuotationApproval = await AsyncExecuter.FirstOrDefaultAsync(
            (await QuotationApprovalRepository.GetQueryableAsync())
                .Where(x =>
                    x.ClaimId == claimId &&
                    x.ClaimFolderId == claimFolderId)
                .OrderByDescending(x => x.CreationTime));

        return latestQuotationApproval == null
            || latestQuotationApproval.Status == ClaimFolderQuotationApprovalStatus.New
            || latestQuotationApproval.Status == ClaimFolderQuotationApprovalStatus.Rejected
            || latestQuotationApproval.Status == ClaimFolderQuotationApprovalStatus.Cancelled;
    }

    private async Task ReplaceDetailedAssessmentStructureAsync(
        ClaimFolder claimFolder,
        ClaimIncident incident,
        ClaimIncidentRiskMotor? riskMotor,
        SaveDetailedAssessmentInput input)
    {
        await ValidateDetailedAssessmentStructureResolvableAsync(input, incident);

        var existingItemQuery = await ClaimFolderItemRepository.GetQueryableAsync();
        var existingItems = await AsyncExecuter.ToListAsync(
            existingItemQuery.Where(x => x.ClaimFolderId == claimFolder.Id));
        var existingItemById = existingItems.ToDictionary(x => x.Id, x => x);
        var existingItemIds = existingItems.Select(x => x.Id).ToList();
        if (existingItemIds.Count > 0)
        {
            var itemPlanQuery = await ClaimFolderItemPlanRepository.GetQueryableAsync();
            var itemPlans = await AsyncExecuter.ToListAsync(
                itemPlanQuery.Where(x => existingItemIds.Contains(x.ClaimFolderItemId)));
            foreach (var itemPlan in itemPlans)
            {
                await ClaimFolderItemPlanRepository.DeleteAsync(itemPlan);
            }
        }

        var exposureQuery = await ClaimFolderExposureRepository.GetQueryableAsync();
        var existingExposures = await AsyncExecuter.ToListAsync(
            exposureQuery.Where(x => x.ClaimFolderId == claimFolder.Id));
        var existingExposureById = existingExposures.ToDictionary(x => x.Id, x => x);

        var incidentObjectQuery = await ClaimFolderIncidentObjectRepository.GetQueryableAsync();
        var existingIncidentObjects = await AsyncExecuter.ToListAsync(
            incidentObjectQuery.Where(x => x.ClaimFolderId == claimFolder.Id));
        var existingIncidentObjectById = existingIncidentObjects.ToDictionary(x => x.Id, x => x);

        var targetExposureIds = new HashSet<Guid>();
        var targetIncidentObjectIds = new HashSet<Guid>();

        foreach (var section in input.Sections ?? new List<DetailedAssessmentSectionDto>())
        {
            var objectTypeId = ParseNullableGuid(section.ObjectTypeId) ?? incident.ObjectTypeId;
            var objectKind = GetObjectKind(section.ObjectTypeCode, section.ObjectTypeName, section.ObjectTypeGroup);
            var coverageCode = await ResolveCoverageCodeAsync(section);
            var sectionItems = section.Items ?? new List<DetailedAssessmentItemDto>();

            ClaimFolderIncidentObject? sharedIncidentObject = null;
            ClaimFolderExposure? sharedExposure = null;
            if (objectKind != "person" && sectionItems.Any(inputItem => ParseNullableGuid(inputItem.ItemId).HasValue))
            {
                sharedExposure = ResolveReusableExposureForSection(section, objectTypeId, coverageCode, existingItems, existingExposureById, existingIncidentObjectById);
                if (sharedExposure != null
                    && existingIncidentObjectById.TryGetValue(sharedExposure.ClaimFolderIncidentObjectId, out var reusableIncidentObject))
                {
                    sharedIncidentObject = reusableIncidentObject;
                    UpdateIncidentObjectForSection(sharedIncidentObject, objectTypeId, objectKind, riskMotor, new DetailedAssessmentItemDto());
                    await ClaimFolderIncidentObjectRepository.UpdateAsync(sharedIncidentObject, autoSave: true);
                }
                else
                {
                    sharedIncidentObject = CreateIncidentObjectForSection(
                        claimFolder.Id,
                        objectTypeId,
                        objectKind,
                        riskMotor,
                        new DetailedAssessmentItemDto());
                    await ClaimFolderIncidentObjectRepository.InsertAsync(sharedIncidentObject, autoSave: true);
                }

                if (sharedExposure != null)
                {
                    sharedExposure.UpdateClaimFolderIncidentObjectId(sharedIncidentObject.Id);
                    sharedExposure.UpdateCoverageCode(coverageCode);
                    sharedExposure.UpdateCoverageParentCode(coverageCode);
                    sharedExposure.UpdateInsurerCoverageCode(coverageCode);
                    await ClaimFolderExposureRepository.UpdateAsync(sharedExposure, autoSave: true);
                }
                else
                {
                    sharedExposure = new ClaimFolderExposure(
                        GuidGenerator.Create(),
                        claimFolder.Id,
                        sharedIncidentObject.Id,
                        coverageCode);
                    sharedExposure.UpdateCoverageParentCode(coverageCode);
                    sharedExposure.UpdateInsurerCoverageCode(coverageCode);
                    await ClaimFolderExposureRepository.InsertAsync(sharedExposure, autoSave: true);
                }

                targetIncidentObjectIds.Add(sharedIncidentObject.Id);
                targetExposureIds.Add(sharedExposure.Id);
            }

            foreach (var inputItem in sectionItems)
            {
                var itemId = await ResolveItemIdAsync(inputItem, objectTypeId, objectKind);
                if (!itemId.HasValue)
                {
                    if (objectKind != "person")
                    {
                        continue;
                    }
                }

                var claimFolderItemId = ResolveDetailedItemAggregateId(inputItem.Id);
                inputItem.Id = claimFolderItemId.ToString();
                var existingItem = existingItemById.TryGetValue(claimFolderItemId, out var item)
                    ? item
                    : null;

                ClaimFolderIncidentObject incidentObject;
                if (sharedIncidentObject != null)
                {
                    incidentObject = sharedIncidentObject;
                }
                else if (existingItem != null
                    && existingIncidentObjectById.TryGetValue(existingItem.ClaimFolderIncidentObjectId, out var currentIncidentObject))
                {
                    incidentObject = currentIncidentObject;
                    UpdateIncidentObjectForSection(incidentObject, objectTypeId, objectKind, riskMotor, inputItem);
                    await ClaimFolderIncidentObjectRepository.UpdateAsync(incidentObject, autoSave: true);
                }
                else
                {
                    incidentObject = CreateIncidentObjectForSection(
                        claimFolder.Id,
                        objectTypeId,
                        objectKind,
                        riskMotor,
                        inputItem);
                    await ClaimFolderIncidentObjectRepository.InsertAsync(incidentObject, autoSave: true);
                }

                ClaimFolderExposure exposure;
                var exposureExists = false;
                if (sharedExposure != null)
                {
                    exposure = sharedExposure;
                    exposureExists = true;
                }
                else if (existingItem != null
                    && existingExposureById.TryGetValue(existingItem.ClaimFolderExposureId, out var currentExposure))
                {
                    exposure = currentExposure;
                    exposureExists = true;
                    exposure.UpdateClaimFolderIncidentObjectId(incidentObject.Id);
                    exposure.UpdateCoverageCode(coverageCode);
                }
                else
                {
                    exposure = new ClaimFolderExposure(
                        GuidGenerator.Create(),
                        claimFolder.Id,
                        incidentObject.Id,
                        coverageCode);
                }

                exposure.UpdateCoverageParentCode(coverageCode);
                exposure.UpdateInsurerCoverageCode(coverageCode);
                if (exposureExists)
                {
                    await ClaimFolderExposureRepository.UpdateAsync(exposure, autoSave: true);
                }
                else
                {
                    await ClaimFolderExposureRepository.InsertAsync(exposure, autoSave: true);
                }

                targetIncidentObjectIds.Add(incidentObject.Id);
                targetExposureIds.Add(exposure.Id);

                var quantity = inputItem.Quantity ?? 1;
                var claimFolderItem = existingItem != null
                    ? existingItem
                    : new ClaimFolderItem(
                        claimFolderItemId,
                        claimFolder.Id,
                        incidentObject.Id,
                        exposure.Id,
                        itemId,
                        quantity);

                claimFolderItem.UpdateClaimFolderIncidentObjectId(incidentObject.Id);
                claimFolderItem.UpdateClaimFolderExposureId(exposure.Id);
                claimFolderItem.UpdateItemId(itemId);
                claimFolderItem.UpdateQuantity(quantity);
                claimFolderItem.UpdateRiskId(ParseNullableGuid(inputItem.RiskId));
                claimFolderItem.UpdateClaimPlanId(objectKind == "person" ? null : ParseNullableGuid(inputItem.ClaimPlanId));
                claimFolderItem.UpdateCoveragePercent(objectKind == "person" ? ParseNullableDouble(inputItem.ClaimPlanId) : null);
                claimFolderItem.UpdateDescription(BuildItemDescription(objectKind, inputItem));
                claimFolderItem.UpdateIssueDate(inputItem.IssueDate);
                claimFolderItem.UpdateIsRecovery(inputItem.IsRecovery ? "Y" : "N");
                claimFolderItem.UpdateIsGenuien(NormalizeYn(inputItem.IsGenuine));

                if (existingItem == null)
                {
                    await ClaimFolderItemRepository.InsertAsync(claimFolderItem, autoSave: true);
                }
                else
                {
                    await ClaimFolderItemRepository.UpdateAsync(claimFolderItem, autoSave: true);
                }

                if (objectKind != "person" && claimFolderItem.ClaimPlanId.HasValue)
                {
                    var itemPlan = new ClaimFolderItemPlan(
                        GuidGenerator.Create(),
                        claimFolderItem.Id,
                        claimFolderItem.ClaimPlanId.Value);
                    await ClaimFolderItemPlanRepository.InsertAsync(itemPlan, autoSave: true);
                }
            }
        }

        var targetItemIds = (input.Sections ?? new List<DetailedAssessmentSectionDto>())
            .SelectMany(section => section.Items ?? new List<DetailedAssessmentItemDto>())
            .Select(item => ParseNullableGuid(item.Id))
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .ToHashSet();

        foreach (var item in existingItems.Where(x => !targetItemIds.Contains(x.Id)))
        {
            await ClaimFolderItemRepository.DeleteAsync(item);
        }

        foreach (var exposure in existingExposures)
        {
            if (!targetExposureIds.Contains(exposure.Id))
            {
                await ClaimFolderExposureRepository.DeleteAsync(exposure);
            }
        }

        foreach (var incidentObject in existingIncidentObjects)
        {
            if (!targetIncidentObjectIds.Contains(incidentObject.Id))
            {
                await ClaimFolderIncidentObjectRepository.DeleteAsync(incidentObject);
            }
        }
    }

    private static ClaimFolderExposure? ResolveReusableExposureForSection(
        DetailedAssessmentSectionDto section,
        Guid objectTypeId,
        string coverageCode,
        List<ClaimFolderItem> existingItems,
        Dictionary<Guid, ClaimFolderExposure> existingExposureById,
        Dictionary<Guid, ClaimFolderIncidentObject> existingIncidentObjectById)
    {
        var sectionExposureId = ParseNullableGuid(section.Id);
        if (sectionExposureId.HasValue
            && existingExposureById.TryGetValue(sectionExposureId.Value, out var sectionExposure)
            && IsExposureMatchedToSection(sectionExposure, objectTypeId, coverageCode, existingIncidentObjectById))
        {
            return sectionExposure;
        }

        foreach (var inputItem in section.Items ?? new List<DetailedAssessmentItemDto>())
        {
            var existingItemId = ParseNullableGuid(inputItem.Id);
            if (!existingItemId.HasValue)
            {
                continue;
            }

            var existingItem = existingItems.FirstOrDefault(x => x.Id == existingItemId.Value);
            if (existingItem == null
                || !existingExposureById.TryGetValue(existingItem.ClaimFolderExposureId, out var itemExposure))
            {
                continue;
            }

            if (IsExposureMatchedToSection(itemExposure, objectTypeId, coverageCode, existingIncidentObjectById))
            {
                return itemExposure;
            }
        }

        return existingExposureById.Values.FirstOrDefault(exposure =>
            IsExposureMatchedToSection(exposure, objectTypeId, coverageCode, existingIncidentObjectById));
    }

    private static bool IsExposureMatchedToSection(
        ClaimFolderExposure exposure,
        Guid objectTypeId,
        string coverageCode,
        Dictionary<Guid, ClaimFolderIncidentObject> existingIncidentObjectById)
    {
        if (!string.Equals(exposure.CoverageCode, coverageCode, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return existingIncidentObjectById.TryGetValue(exposure.ClaimFolderIncidentObjectId, out var incidentObject)
            && incidentObject.ObjectTypeId == objectTypeId;
    }

    private async Task ValidateDetailedAssessmentStructureResolvableAsync(
        SaveDetailedAssessmentInput input,
        ClaimIncident incident)
    {
        var sections = input.Sections ?? new List<DetailedAssessmentSectionDto>();
        Logger.LogInformation(
            "[DetailedAssessment][Save][InputSections] SectionCount={SectionCount}, Sections={Sections}",
            sections.Count,
            string.Join(" | ", sections.Select(section =>
                $"CoverageId={section.CoverageId};CoverageName={section.CoverageName};ObjectTypeId={section.ObjectTypeId};ObjectType={section.ObjectTypeCode}-{section.ObjectTypeName}-{section.ObjectTypeGroup};ItemCount={section.Items?.Count ?? 0}")));

        foreach (var section in sections)
        {
            var objectTypeId = ParseNullableGuid(section.ObjectTypeId) ?? incident.ObjectTypeId;
            var objectKind = GetObjectKind(section.ObjectTypeCode, section.ObjectTypeName, section.ObjectTypeGroup);

            foreach (var inputItem in section.Items ?? new List<DetailedAssessmentItemDto>())
            {
                var itemId = await ResolveItemIdAsync(inputItem, objectTypeId, objectKind);
                Logger.LogInformation(
                    "[DetailedAssessment][Save][ResolveItem] CoverageId={CoverageId}, CoverageName={CoverageName}, ObjectTypeId={ObjectTypeId}, ObjectKind={ObjectKind}, InputItemId={InputItemId}, ResolvedItemId={ResolvedItemId}, PersonName={PersonName}",
                    section.CoverageId,
                    section.CoverageName,
                    objectTypeId,
                    objectKind,
                    inputItem.ItemId,
                    itemId,
                    inputItem.PersonName);

            }
        }
    }

    private async Task UpsertDetailedItemDocumentsAsync(
        Guid claimId,
        Guid claimFolderId,
        List<DetailedAssessmentSectionDto>? sections)
    {
        var targetItems = (sections ?? new List<DetailedAssessmentSectionDto>())
            .SelectMany(section => section.Items ?? new List<DetailedAssessmentItemDto>())
            .Select(item => new
            {
                ItemId = ParseNullableGuid(item.Id),
                Attachments = item.Attachments ?? new List<DetailedAssessmentAttachmentDto>()
            })
            .Where(x => x.ItemId.HasValue)
            .Select(x => new
            {
                ItemId = x.ItemId!.Value,
                Attachments = x.Attachments
                    .Where(file => ParseNullableGuid(file.DocumentId).HasValue)
                    .ToList()
            })
            .ToList();

        var existingClaimDocs = await AsyncExecuter.ToListAsync(
            (await ClaimDocumentRepository.GetQueryableAsync())
            .Where(x => x.ClaimFolderId == claimFolderId && x.ClaimFolderItemId.HasValue)
            .OrderBy(x => x.CreationTime));

        foreach (var claimDoc in existingClaimDocs)
        {
            if (!claimDoc.ClaimFolderItemId.HasValue)
            {
                continue;
            }

            var targetItem = targetItems.FirstOrDefault(x => x.ItemId == claimDoc.ClaimFolderItemId.Value);
            var shouldKeep = targetItem != null
                && claimDoc.DocumentId.HasValue
                && targetItem.Attachments.Any(file =>
                    string.Equals(file.DocumentId, claimDoc.DocumentId.Value.ToString(), StringComparison.OrdinalIgnoreCase));

            if (!shouldKeep)
            {
                await DeleteClaimDocumentWithResourceAsync(claimDoc);
            }
        }

        foreach (var targetItem in targetItems)
        {
            var remainingClaimDocs = existingClaimDocs
                .Where(x => x.ClaimFolderItemId == targetItem.ItemId)
                .ToList();

            foreach (var attachment in targetItem.Attachments)
            {
                var documentId = ParseNullableGuid(attachment.DocumentId);
                if (!documentId.HasValue)
                {
                    continue;
                }

                var claimDocumentId = ParseNullableGuid(attachment.ClaimDocumentId);
                var existingClaimDoc = claimDocumentId.HasValue
                    ? remainingClaimDocs.FirstOrDefault(x => x.Id == claimDocumentId.Value)
                    : remainingClaimDocs.FirstOrDefault(x => x.DocumentId == documentId.Value);

                if (existingClaimDoc != null)
                {
                    continue;
                }

                var claimDocument = new ClaimDocument(
                    GuidGenerator.Create(),
                    claimId: claimId,
                    documentId: documentId.Value,
                    claimFolderId: claimFolderId,
                    claimFolderItemId: targetItem.ItemId);
                await ClaimDocumentRepository.InsertAsync(claimDocument, autoSave: true);
            }
        }
    }

    private async Task<List<DetailedAssessmentDocumentRowDto>> BuildDocumentRowsAsync(Guid? claimFolderId)
    {
        var rows = await BuildConfiguredDocumentRowsAsync();
        if (!claimFolderId.HasValue)
        {
            Logger.LogWarning("[DetailedAssessment][Documents] ClaimFolderId=null, skip loading claim_document to avoid mixing documents from other claim folders.");
            return rows;
        }

        var rowByCode = rows
            .Where(x => !string.IsNullOrWhiteSpace(x.Code))
            .GroupBy(x => NormalizeDetailedAssessmentDocTypeCode(x.Code), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(x => x.Key, x => x.First(), StringComparer.OrdinalIgnoreCase);

        var claimDocs = await AsyncExecuter.ToListAsync(
            (await ClaimDocumentRepository.GetQueryableAsync())
            .Where(x => x.ClaimFolderItemId == null && x.ClaimFolderId == claimFolderId.Value)
            .OrderBy(x => x.CreationTime));
        var documentIds = claimDocs
            .Where(x => x.DocumentId.HasValue)
            .Select(x => x.DocumentId!.Value)
            .Distinct()
            .ToList();
        var resDocs = documentIds.Count == 0
            ? new List<ResDocument>()
            : await AsyncExecuter.ToListAsync(
                (await ResDocumentRepository.GetQueryableAsync()).Where(x => documentIds.Contains(x.Id)));
        var resDocById = resDocs.ToDictionary(x => x.Id, x => x);
        var typeIds = claimDocs
            .Where(x => x.DocumentTypeId.HasValue)
            .Select(x => x.DocumentTypeId!.Value)
            .Concat(resDocs.Select(x => x.DocumentTypeId))
            .Distinct()
            .ToList();
        var typeById = typeIds.Count == 0
            ? new Dictionary<Guid, ResDocumentType>()
            : (await AsyncExecuter.ToListAsync(
                (await ResDocumentTypeRepository.GetQueryableAsync()).Where(x => typeIds.Contains(x.Id))))
            .ToDictionary(x => x.Id, x => x);

        foreach (var claimDoc in claimDocs)
        {
            ResDocument? resDoc = null;
            if (claimDoc.DocumentId.HasValue)
            {
                resDocById.TryGetValue(claimDoc.DocumentId.Value, out resDoc);
            }

            var docTypeId = claimDoc.DocumentTypeId ?? resDoc?.DocumentTypeId;
            if (!docTypeId.HasValue || !typeById.TryGetValue(docTypeId.Value, out var docType))
            {
                continue;
            }

            var docCode = NormalizeDetailedAssessmentDocTypeCode(docType?.Code);
            if (!rowByCode.TryGetValue(docCode, out var row))
                continue;

            row.Complete = FromYn(claimDoc.Complete);
            row.IsCopy = FromYn(claimDoc.IsCopy);
            row.Note = claimDoc.Note;
            row.IssueDate = claimDoc.IssueDate;
            if (resDoc == null || string.Equals(resDoc.GroupCode, DetailedProfileMetadataGroupCode, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            row.Attachments.Add(new DetailedAssessmentAttachmentDto
            {
                Id = claimDoc.Id.ToString(),
                ClaimDocumentId = claimDoc.Id.ToString(),
                DocumentId = claimDoc.DocumentId!.Value.ToString(),
                FileName = resDoc.FileName ?? docType?.Name ?? row.Name,
                Url = resDoc.Url
            });
        }

        return rows;
    }

    private async Task UpsertDetailedDocumentsAsync(
        Guid claimId,
        Guid claimFolderId,
        List<DetailedAssessmentDocumentRowDto>? rows)
    {
        if (rows == null || rows.Count == 0)
        {
            return;
        }

        var rowKeys = rows
            .Select(row => new
            {
                Row = row,
                Key = GetDetailedDocumentCode(row)
            })
            .ToList();
        Logger.LogInformation(
            "[DetailedAssessment][Documents][Upsert][v2-dynamic-document-types] ClaimId={ClaimId}, ClaimFolderId={ClaimFolderId}, RawRowCount={RawRowCount}, Rows={Rows}",
            claimId,
            claimFolderId,
            rows.Count,
            string.Join(" | ", rowKeys.Select(x =>
                $"Id={x.Row.Id};Code={x.Row.Code};DocumentTypeId={x.Row.DocumentTypeId};Name={x.Row.Name};Key={x.Key};AttachmentCount={x.Row.Attachments?.Count ?? 0}")));

        var duplicatedKeys = rowKeys
            .GroupBy(x => x.Key, StringComparer.OrdinalIgnoreCase)
            .Where(x => x.Count() > 1)
            .Select(x => $"{x.Key}:{x.Count()}")
            .ToList();
        if (duplicatedKeys.Count > 0)
        {
            Logger.LogWarning(
                "[DetailedAssessment][Documents][Upsert][v2-dynamic-document-types] Duplicate document row keys detected before merge. Keys={Keys}",
                string.Join(", ", duplicatedKeys));
        }

        var normalizedRows = rowKeys
            .GroupBy(x => x.Key, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(x => x.Key, x => MergeDetailedDocumentRows(x.Select(y => y.Row)), StringComparer.OrdinalIgnoreCase);
        Logger.LogInformation(
            "[DetailedAssessment][Documents][Upsert][v2-dynamic-document-types] NormalizedRowCount={NormalizedRowCount}, Keys={Keys}",
            normalizedRows.Count,
            string.Join(", ", normalizedRows.Keys));
        var claimDocs = await AsyncExecuter.ToListAsync(
            (await ClaimDocumentRepository.GetQueryableAsync())
            .Where(x => x.ClaimFolderItemId == null && x.ClaimFolderId == claimFolderId));
        var documentIds = claimDocs
            .Where(x => x.DocumentId.HasValue)
            .Select(x => x.DocumentId!.Value)
            .Distinct()
            .ToList();
        var resDocs = documentIds.Count == 0
            ? new List<ResDocument>()
            : await AsyncExecuter.ToListAsync(
                (await ResDocumentRepository.GetQueryableAsync())
                .Where(x => documentIds.Contains(x.Id)));
        var typeIds = claimDocs
            .Where(x => x.DocumentTypeId.HasValue)
            .Select(x => x.DocumentTypeId!.Value)
            .Concat(resDocs.Select(x => x.DocumentTypeId))
            .Distinct()
            .ToList();
        var docTypes = typeIds.Count == 0
            ? new List<ResDocumentType>()
            : await AsyncExecuter.ToListAsync(
                (await ResDocumentTypeRepository.GetQueryableAsync()).Where(x => typeIds.Contains(x.Id)));
        var docTypeById = docTypes.ToDictionary(x => x.Id, x => x);
        var documentTypes = await AsyncExecuter.ToListAsync(
            (await ResDocumentTypeRepository.GetQueryableAsync())
            .Where(x => x.DocumentGroupCode != null && x.DocumentGroupCode == ProfileDocumentGroupCode));

        var inputDocumentIds = normalizedRows.Values
            .SelectMany(row => row.Attachments ?? new List<DetailedAssessmentAttachmentDto>())
            .Select(file => ParseNullableGuid(file.DocumentId))
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();
        var missingInputDocumentIds = inputDocumentIds
            .Where(id => resDocs.All(x => x.Id != id))
            .ToList();
        if (missingInputDocumentIds.Count > 0)
        {
            var inputResDocs = await AsyncExecuter.ToListAsync(
                (await ResDocumentRepository.GetQueryableAsync())
                .Where(x => missingInputDocumentIds.Contains(x.Id)));
            resDocs.AddRange(inputResDocs);
        }

        foreach (var pair in normalizedRows)
        {
            var code = pair.Key;
            var row = pair.Value;
            var targetDocType = ResolveDetailedDocumentType(row, code, documentTypes);
            if (targetDocType == null)
            {
                continue;
            }

            var metadataClaimDoc = claimDocs.FirstOrDefault(x =>
                x.DocumentId == null && x.DocumentTypeId == targetDocType.Id);
            if (metadataClaimDoc == null)
            {
                metadataClaimDoc = new ClaimDocument(
                    GuidGenerator.Create(),
                    claimId: claimId,
                    documentTypeId: targetDocType.Id,
                    claimFolderId: claimFolderId,
                    note: string.IsNullOrWhiteSpace(row.Note) ? null : row.Note.Trim(),
                    complete: ToYn(row.Complete),
                    isCopy: ToYn(row.IsCopy),
                    issueDate: row.IssueDate);
                await ClaimDocumentRepository.InsertAsync(metadataClaimDoc, autoSave: true);
                claimDocs.Add(metadataClaimDoc);
            }
            else if (metadataClaimDoc != null)
            {
                metadataClaimDoc.UpdateDocumentTypeId(targetDocType.Id);
                metadataClaimDoc.UpdateClaimFolderId(claimFolderId);
                metadataClaimDoc.UpdateMetadata(
                    string.IsNullOrWhiteSpace(row.Note) ? null : row.Note.Trim(),
                    ToYn(row.Complete),
                    ToYn(row.IsCopy),
                    row.IssueDate);
                await ClaimDocumentRepository.UpdateAsync(metadataClaimDoc);
            }

            var rowAttachmentDocumentIds = (row.Attachments ?? new List<DetailedAssessmentAttachmentDto>())
                .Select(file => ParseNullableGuid(file.DocumentId))
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .ToHashSet();
            foreach (var resDoc in resDocs.Where(x => rowAttachmentDocumentIds.Contains(x.Id)))
            {
                if (resDoc.DocumentTypeId != targetDocType.Id)
                {
                    Logger.LogInformation(
                        "[DetailedAssessment][Documents][Upsert][v2-dynamic-document-types] Reassign attachment document type. DocumentId={DocumentId}, FromTypeId={FromTypeId}, ToTypeId={ToTypeId}, RowCode={RowCode}, RowName={RowName}",
                        resDoc.Id,
                        resDoc.DocumentTypeId,
                        targetDocType.Id,
                        code,
                        row.Name);
                    resDoc.UpdateDocumentTypeId(targetDocType.Id);
                }

                if (!string.Equals(resDoc.GroupCode, code, StringComparison.OrdinalIgnoreCase))
                {
                    resDoc.UpdateGroupCode(code);
                }

                await ResDocumentRepository.UpdateAsync(resDoc);
                docTypeById[targetDocType.Id] = targetDocType;
            }
        }

        foreach (var pair in normalizedRows)
        {
            var code = pair.Key;
            var row = pair.Value;
            var rowDocumentIds = (row.Attachments ?? new List<DetailedAssessmentAttachmentDto>())
                .Select(file => ParseNullableGuid(file.DocumentId))
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .ToHashSet();

            var claimDocsToDelete = claimDocs
                .Where(claimDoc =>
                {
                    if (!claimDoc.DocumentId.HasValue)
                    {
                        return false;
                    }

                    var resDoc = resDocs.FirstOrDefault(x => x.Id == claimDoc.DocumentId.Value);
                    if (resDoc == null || !docTypeById.TryGetValue(resDoc.DocumentTypeId, out var docType))
                    {
                        return false;
                    }

                    if (string.Equals(resDoc.GroupCode, DetailedProfileMetadataGroupCode, StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }

                    return string.Equals(NormalizeDetailedAssessmentDocTypeCode(docType.Code), code, StringComparison.OrdinalIgnoreCase)
                        && !rowDocumentIds.Contains(claimDoc.DocumentId.Value);
                })
                .ToList();

            foreach (var claimDoc in claimDocsToDelete)
            {
                await DeleteClaimDocumentWithResourceAsync(claimDoc);
                claimDocs.Remove(claimDoc);
            }
        }

        foreach (var claimDoc in claimDocs)
        {
            if (!claimDoc.DocumentId.HasValue)
            {
                continue;
            }

            var resDoc = resDocs.FirstOrDefault(x => x.Id == claimDoc.DocumentId.Value);
            if (resDoc == null || !docTypeById.TryGetValue(resDoc.DocumentTypeId, out var docType))
            {
                continue;
            }

            var code = NormalizeDetailedAssessmentDocTypeCode(docType.Code);
            if (!normalizedRows.TryGetValue(code, out var row))
            {
                continue;
            }

            claimDoc.UpdateMetadata(
                string.IsNullOrWhiteSpace(row.Note) ? null : row.Note.Trim(),
                ToYn(row.Complete),
                ToYn(row.IsCopy),
                row.IssueDate);
            claimDoc.UpdateClaimFolderId(claimFolderId);
            claimDoc.UpdateDocumentTypeId(docType.Id);
            await ClaimDocumentRepository.UpdateAsync(claimDoc);
        }

        foreach (var pair in normalizedRows)
        {
            var code = pair.Key;
            var row = pair.Value;
            var attachmentDocType = ResolveDetailedDocumentType(row, code, documentTypes);
            var rowAttachmentDocumentIds = (row.Attachments ?? new List<DetailedAssessmentAttachmentDto>())
                .Select(file => ParseNullableGuid(file.DocumentId))
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .ToHashSet();

            foreach (var documentId in rowAttachmentDocumentIds)
            {
                var existingAttachmentClaimDoc = claimDocs.FirstOrDefault(claimDoc => claimDoc.DocumentId == documentId)
                    ?? claimDocs.FirstOrDefault(claimDoc =>
                {
                    if (!claimDoc.DocumentId.HasValue || claimDoc.DocumentId.Value != documentId)
                    {
                        return false;
                    }

                    var resDoc = resDocs.FirstOrDefault(x => x.Id == claimDoc.DocumentId.Value);
                    if (resDoc == null || !docTypeById.TryGetValue(resDoc.DocumentTypeId, out var docType))
                    {
                        return false;
                    }

                        return string.Equals(NormalizeDetailedAssessmentDocTypeCode(docType.Code), code, StringComparison.OrdinalIgnoreCase);
                    });

                if (existingAttachmentClaimDoc != null)
                {
                    existingAttachmentClaimDoc.UpdateMetadata(
                        string.IsNullOrWhiteSpace(row.Note) ? null : row.Note.Trim(),
                        ToYn(row.Complete),
                        ToYn(row.IsCopy),
                        row.IssueDate);
                    existingAttachmentClaimDoc.UpdateClaimFolderId(claimFolderId);
                    existingAttachmentClaimDoc.UpdateDocumentTypeId(attachmentDocType?.Id);
                    await ClaimDocumentRepository.UpdateAsync(existingAttachmentClaimDoc);
                    continue;
                }

                var attachmentClaimDocument = new ClaimDocument(
                    GuidGenerator.Create(),
                    claimId: claimId,
                    documentId: documentId,
                    documentTypeId: attachmentDocType?.Id,
                    claimFolderId: claimFolderId,
                    note: string.IsNullOrWhiteSpace(row.Note) ? null : row.Note.Trim(),
                    complete: ToYn(row.Complete),
                    isCopy: ToYn(row.IsCopy),
                    issueDate: row.IssueDate);
                await ClaimDocumentRepository.InsertAsync(attachmentClaimDocument, autoSave: true);
                claimDocs.Add(attachmentClaimDocument);
            }

        }
    }

    private static string NormalizeDetailedAssessmentDocTypeCode(string? docTypeCode)
    {
        if (string.IsNullOrWhiteSpace(docTypeCode))
            return "OTHER";

        return docTypeCode.Trim().ToUpperInvariant();
    }

    private static ResDocumentType? ResolveDetailedDocumentType(
        DetailedAssessmentDocumentRowDto row,
        string code,
        List<ResDocumentType> documentTypes)
    {
        var rowDocumentTypeId = ParseNullableGuid(row.DocumentTypeId);
        if (rowDocumentTypeId.HasValue)
        {
            var matchedById = documentTypes.FirstOrDefault(x => x.Id == rowDocumentTypeId.Value);
            if (matchedById != null)
            {
                return matchedById;
            }
        }

        return documentTypes.FirstOrDefault(x =>
            string.Equals(NormalizeDetailedAssessmentDocTypeCode(x.Code), code, StringComparison.OrdinalIgnoreCase));
    }

    private static Guid ResolveDetailedItemAggregateId(string? id)
    {
        var parsedId = ParseNullableGuid(id);
        return parsedId ?? Guid.NewGuid();
    }

    private async Task DeleteClaimDocumentWithResourceAsync(ClaimDocument claimDocument)
    {
        var documentId = claimDocument.DocumentId;
        await ClaimDocumentRepository.DeleteAsync(claimDocument);

        if (!documentId.HasValue)
        {
            return;
        }

        var otherReferences = await AsyncExecuter.CountAsync(
            (await ClaimDocumentRepository.GetQueryableAsync()).Where(x => x.DocumentId == documentId.Value));
        if (otherReferences > 0)
        {
            return;
        }

        var resDocument = await ResDocumentRepository.FirstOrDefaultAsync(x => x.Id == documentId.Value);
        if (resDocument != null)
        {
            await ResDocumentRepository.DeleteAsync(resDocument);
        }
    }

    private static string GetObjectKind(string? objectTypeCode, string? objectTypeName, string? objectTypeGroup)
    {
        var source = RemoveDiacritics($"{objectTypeCode} {objectTypeName} {objectTypeGroup}".ToLowerInvariant())
            .Replace('đ', 'd');
        if (source.Contains("nguoi") || source.Contains("person") || source.Contains("human") || source.Contains("con nguoi"))
        {
            return "person";
        }

        if (source.Contains("xe") || source.Contains("oto") || source.Contains("motor") || source.Contains("car") || source.Contains("vehicle"))
        {
            return "vehicle";
        }

        return "asset";
    }

    private static string RemoveDiacritics(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var normalized = value.Normalize(NormalizationForm.FormD);
        var chars = normalized
            .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            .ToArray();
        return new string(chars).Normalize(NormalizationForm.FormC);
    }

    private async Task<string> ResolveCoverageCodeAsync(DetailedAssessmentSectionDto section)
    {
        if (Guid.TryParse(section.CoverageId, out var coverageId) && coverageId != Guid.Empty)
        {
            var coverage = await ProCoverageRepository.FirstOrDefaultAsync(x => x.Id == coverageId);
            if (coverage != null)
            {
                return coverage.Code;
            }
        }

        return GetCoverageCodeById(section.CoverageId);
    }

    private async Task<Guid?> ResolveItemIdAsync(DetailedAssessmentItemDto inputItem, Guid objectTypeId, string objectKind)
    {
        if (Guid.TryParse(inputItem.ItemId, out var itemId) && itemId != Guid.Empty)
        {
            return itemId;
        }

        if (objectKind != "person")
        {
            return null;
        }

        var firstMatchingItem = await AsyncExecuter.FirstOrDefaultAsync(
            (await ResObjectTypeItemRepository.GetQueryableAsync())
            .Where(x => x.ObjectTypeId == objectTypeId)
            .OrderBy(x => x.Name));
        if (firstMatchingItem != null)
        {
            return firstMatchingItem.Id;
        }

        var objectTypeQuery = await ResObjectTypeRepository.GetQueryableAsync();
        var personObjectTypes = await AsyncExecuter.ToListAsync(objectTypeQuery);
        var personObjectTypeIds = personObjectTypes
            .Where(x => GetObjectKind(x.Code, x.Name, x.ObjectGroup) == "person")
            .Select(x => x.Id)
            .ToList();

        if (personObjectTypeIds.Count == 0)
        {
            return null;
        }

        var fallbackItem = await AsyncExecuter.FirstOrDefaultAsync(
            (await ResObjectTypeItemRepository.GetQueryableAsync())
            .Where(x => personObjectTypeIds.Contains(x.ObjectTypeId))
            .OrderBy(x => x.Name));

        return fallbackItem?.Id;
    }

    private ClaimFolderIncidentObject CreateIncidentObjectForSection(
        Guid claimFolderId,
        Guid objectTypeId,
        string objectKind,
        ClaimIncidentRiskMotor? riskMotor,
        DetailedAssessmentItemDto inputItem)
    {
        if (objectKind == "person")
        {
            return new ClaimFolderIncidentObject(
                GuidGenerator.Create(),
                claimFolderId,
                objectTypeId,
                name: string.IsNullOrWhiteSpace(inputItem.PersonName) ? null : inputItem.PersonName.Trim(),
                idNo: string.IsNullOrWhiteSpace(inputItem.PersonIdNo) ? null : inputItem.PersonIdNo.Trim(),
                exitDate: inputItem.DischargeDate);
        }

        return new ClaimFolderIncidentObject(
            GuidGenerator.Create(),
            claimFolderId,
            objectTypeId,
            carPlate: riskMotor?.CarPlate,
            carEngineNumber: riskMotor?.EngineNumber,
            carVin: riskMotor?.Vin,
            name: riskMotor?.DriverName,
            idNo: riskMotor?.DriverIdNo);
    }

    private static void UpdateIncidentObjectForSection(
        ClaimFolderIncidentObject incidentObject,
        Guid objectTypeId,
        string objectKind,
        ClaimIncidentRiskMotor? riskMotor,
        DetailedAssessmentItemDto inputItem)
    {
        if (objectKind == "person")
        {
            incidentObject.UpdateDetailedAssessmentInfo(
                objectTypeId,
                name: string.IsNullOrWhiteSpace(inputItem.PersonName) ? null : inputItem.PersonName.Trim(),
                idNo: string.IsNullOrWhiteSpace(inputItem.PersonIdNo) ? null : inputItem.PersonIdNo.Trim(),
                exitDate: inputItem.DischargeDate);
            return;
        }

        incidentObject.UpdateDetailedAssessmentInfo(
            objectTypeId,
            carPlate: riskMotor?.CarPlate,
            carEngineNumber: riskMotor?.EngineNumber,
            carVin: riskMotor?.Vin,
            name: riskMotor?.DriverName,
            idNo: riskMotor?.DriverIdNo);
    }

    private static string? BuildItemDescription(string objectKind, DetailedAssessmentItemDto inputItem)
    {
        return string.IsNullOrWhiteSpace(inputItem.Description) ? null : inputItem.Description.Trim();
    }

    private static PersonItemPayload ParsePersonPayload(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return new PersonItemPayload();
        }

        try
        {
            if (!description.TrimStart().StartsWith("{", StringComparison.Ordinal))
            {
                return new PersonItemPayload { Description = description };
            }

            return JsonSerializer.Deserialize<PersonItemPayload>(description) ?? new PersonItemPayload();
        }
        catch
        {
            return new PersonItemPayload { Description = description };
        }
    }

    private static Guid? ParseNullableGuid(string? value)
    {
        return Guid.TryParse(value, out var parsed) && parsed != Guid.Empty ? parsed : null;
    }

    private static double? ParseNullableDouble(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var invariantParsed))
        {
            return invariantParsed;
        }

        if (double.TryParse(value, NumberStyles.Any, CultureInfo.GetCultureInfo("vi-VN"), out var viParsed))
        {
            return viParsed;
        }

        return null;
    }

    private static string? FormatCoveragePercent(double? value)
    {
        return value?.ToString(CultureInfo.InvariantCulture);
    }

    private static string GetCoverageCodeById(string? coverageId)
    {
        return coverageId switch
        {
            "coverage-physical" => "VCX",
            "coverage-third-party-property" => "TNDS_TS",
            "coverage-cargo" => "HH_TREN_XE",
            _ => string.IsNullOrWhiteSpace(coverageId) ? "OTHER" : coverageId
        };
    }

    private static string GetCoverageIdByCode(string? coverageCode)
    {
        if (string.Equals(coverageCode, "VCX", StringComparison.OrdinalIgnoreCase))
            return "coverage-physical";
        if (string.Equals(coverageCode, "TNDS_TS", StringComparison.OrdinalIgnoreCase))
            return "coverage-third-party-property";
        if (string.Equals(coverageCode, "HH_TREN_XE", StringComparison.OrdinalIgnoreCase))
            return "coverage-cargo";

        return coverageCode ?? "coverage-physical";
    }

    private static string GetCoverageNameByCode(string? coverageCode)
    {
        if (string.Equals(coverageCode, "VCX", StringComparison.OrdinalIgnoreCase))
            return "Vật chất xe";
        if (string.Equals(coverageCode, "TNDS_TS", StringComparison.OrdinalIgnoreCase))
            return "TNDS BB về tài sản";
        if (string.Equals(coverageCode, "HH_TREN_XE", StringComparison.OrdinalIgnoreCase))
            return "TNDS về hàng hóa trên xe";

        return coverageCode ?? "Khác";
    }

    private static string GetDetailedDocumentCode(DetailedAssessmentDocumentRowDto row)
    {
        if (!string.IsNullOrWhiteSpace(row.Code))
        {
            return NormalizeDetailedAssessmentDocTypeCode(row.Code);
        }

        if (!string.IsNullOrWhiteSpace(row.DocumentTypeId))
        {
            return row.DocumentTypeId.Trim().ToUpperInvariant();
        }

        return string.IsNullOrWhiteSpace(row.Id)
            ? row.Name.Trim().ToUpperInvariant()
            : row.Id.Trim().ToUpperInvariant();
    }

    private static DetailedAssessmentDocumentRowDto MergeDetailedDocumentRows(IEnumerable<DetailedAssessmentDocumentRowDto> rows)
    {
        var rowList = rows.ToList();
        var first = rowList.First();
        if (rowList.Count == 1)
        {
            return first;
        }

        first.Attachments = rowList
            .SelectMany(x => x.Attachments ?? new List<DetailedAssessmentAttachmentDto>())
            .GroupBy(x => x.DocumentId ?? x.Id)
            .Select(x => x.First())
            .ToList();
        first.Complete = MergeNullableBoolean(rowList.Select(x => x.Complete));
        first.IsCopy = MergeNullableBoolean(rowList.Select(x => x.IsCopy));
        first.Note = rowList.Select(x => x.Note).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x)) ?? first.Note;
        first.IssueDate = rowList.Select(x => x.IssueDate).FirstOrDefault(x => x.HasValue) ?? first.IssueDate;

        return first;
    }

    private static string? NormalizeYn(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return string.Equals(value.Trim(), "Y", StringComparison.OrdinalIgnoreCase) ? "Y" : "N";
    }

    private static string? ToYn(bool? value)
    {
        return value.HasValue
            ? value.Value ? "Y" : "N"
            : null;
    }

    private static bool? FromYn(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return string.Equals(value.Trim(), "Y", StringComparison.OrdinalIgnoreCase);
    }

    private static bool? MergeNullableBoolean(IEnumerable<bool?> values)
    {
        var explicitValues = values.Where(x => x.HasValue).Select(x => x!.Value).ToList();
        if (explicitValues.Count == 0)
        {
            return null;
        }

        return explicitValues.Any(x => x);
    }

    private sealed class PersonItemPayload
    {
        public string? Description { get; set; }
        public DateTime? DischargeDate { get; set; }
    }

    private async Task<Guid> GetCurrentEmployeeIdAsync()
    {
        if (CurrentUser.Id == null)
            throw new Volo.Abp.UserFriendlyException(L["UserNotAuthenticated"].Value);
        var employee = await EmployeeRepository.FirstOrDefaultAsync(e => e.UserId == CurrentUser.Id.Value);
        if (employee == null)
            throw new Volo.Abp.UserFriendlyException(L["EmployeeNotFoundForUser"].Value);
        return employee.Id;
    }
}
