using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ClosedXML.Excel;
using iOne.Claim.Localization;
using iOne.ClaimAdjustAtLocations;
using iOne.ClaimDocuments;
using iOne.ClaimIncidentRiskMotors;
using iOne.ClaimIncidents;
using iOne.Claims;
using iOne.HrDepartments;
using iOne.HrEmployees;
using iOne.ResDocuments;
using iOne.ResDocumentTypes;
using iOne.ResPartners;
using iOne.ResTaskCategories;
using iOne.Workflow;
using iOne.WorkInstances;
using iOne.WorkTasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Content;
using Volo.Abp.Domain.Repositories;
using ClaimEntity = iOne.Claims.Claim;

namespace iOne.Claim.Claims;

public class ClaimOnsiteAssessmentTaskAppService : ApplicationService, IClaimOnsiteAssessmentTaskAppService
{
    private const string OnsiteProfileDocumentGroupCode = "CLAIM_ADJUST_ONSITE_PROFILE";

    protected IRepository<WorkTask, Guid> WorkTaskRepository { get; }
    protected IRepository<ResTaskCategory, Guid> ResTaskCategoryRepository { get; }
    protected IRepository<ClaimEntity, Guid> ClaimRepository { get; }
    protected IRepository<ClaimAdjustAtLocation, Guid> ClaimAdjustAtLocationRepository { get; }
    protected IRepository<ClaimIncident, Guid> ClaimIncidentRepository { get; }
    protected IRepository<ClaimIncidentRiskMotor, Guid> ClaimIncidentRiskMotorRepository { get; }
    protected IRepository<HrEmployee, Guid> EmployeeRepository { get; }
    protected IRepository<HrDepartment, Guid> DepartmentRepository { get; }
    protected IRepository<ResPartner, Guid> PartnerRepository { get; }
    protected IRepository<ClaimDocument, Guid> ClaimDocumentRepository { get; }
    protected IRepository<ResDocument, Guid> ResDocumentRepository { get; }
    protected IRepository<ResDocumentType, Guid> ResDocumentTypeRepository { get; }
    private readonly IElsaWorkflowService _elsaWorkflowService;
    private readonly IWorkInstanceRepository _workInstanceRepository;
    private readonly ILogger<ClaimOnsiteAssessmentTaskAppService> _logger;
    private readonly IOnsiteAssessmentAssignmentService _onsiteAssessmentAssignmentService;

    public ClaimOnsiteAssessmentTaskAppService(
        IRepository<WorkTask, Guid> workTaskRepository,
        IRepository<ResTaskCategory, Guid> resTaskCategoryRepository,
        IRepository<ClaimEntity, Guid> claimRepository,
        IRepository<ClaimAdjustAtLocation, Guid> claimAdjustAtLocationRepository,
        IRepository<ClaimIncident, Guid> claimIncidentRepository,
        IRepository<ClaimIncidentRiskMotor, Guid> claimIncidentRiskMotorRepository,
        IRepository<HrEmployee, Guid> employeeRepository,
        IRepository<HrDepartment, Guid> departmentRepository,
        IRepository<ResPartner, Guid> partnerRepository,
        IRepository<ClaimDocument, Guid> claimDocumentRepository,
        IRepository<ResDocument, Guid> resDocumentRepository,
        IRepository<ResDocumentType, Guid> resDocumentTypeRepository,
        IElsaWorkflowService elsaWorkflowService,
        IWorkInstanceRepository workInstanceRepository,
        ILogger<ClaimOnsiteAssessmentTaskAppService> logger,
        IOnsiteAssessmentAssignmentService onsiteAssessmentAssignmentService)
    {
        WorkTaskRepository = workTaskRepository;
        ResTaskCategoryRepository = resTaskCategoryRepository;
        ClaimRepository = claimRepository;
        ClaimAdjustAtLocationRepository = claimAdjustAtLocationRepository;
        ClaimIncidentRepository = claimIncidentRepository;
        ClaimIncidentRiskMotorRepository = claimIncidentRiskMotorRepository;
        EmployeeRepository = employeeRepository;
        DepartmentRepository = departmentRepository;
        PartnerRepository = partnerRepository;
        ClaimDocumentRepository = claimDocumentRepository;
        ResDocumentRepository = resDocumentRepository;
        ResDocumentTypeRepository = resDocumentTypeRepository;
        LocalizationResource = typeof(ClaimResource);
        _elsaWorkflowService = elsaWorkflowService;
        _workInstanceRepository = workInstanceRepository;
        _logger = logger;
        _onsiteAssessmentAssignmentService = onsiteAssessmentAssignmentService;
    }

    public virtual async Task<OnsiteAssessmentDetailDto> GetDetailAsync(Guid workTaskId)
    {
        var currentEmployeeId = await GetCurrentEmployeeIdAsync();
        var workTask = await WorkTaskRepository.GetAsync(workTaskId);

        if (workTask.AssigneeId != currentEmployeeId && workTask.ReporterId != currentEmployeeId)
        {
            throw new Volo.Abp.UserFriendlyException(L["ClaimTask:NotAuthorized"].Value);
        }

        var context = await ResolveWorkTaskContextAsync(workTask);
        return await BuildDetailDtoAsync(context.ClaimId, workTask, context.AdjustAtLocationId, currentEmployeeId);
    }

    public virtual async Task<OnsiteAssessmentDetailDto> GetDetailByClaimIdAsync(Guid claimId)
    {
        var currentEmployeeId = await GetCurrentEmployeeIdAsync();
        return await BuildDetailDtoAsync(claimId, null, null, currentEmployeeId);
    }

    public virtual async Task<PagedResultDto<OnsiteAssessmentTaskDto>> GetListAsync(GetOnsiteAssessmentTasksInput input)
    {
        if (CurrentUser.Id == null)
        {
            throw new Volo.Abp.UserFriendlyException(L["UserNotAuthenticated"].Value);
        }

        var currentEmployeeId = await GetCurrentEmployeeIdAsync();
        var onsiteTaskCategoryId = await GetOnsiteAssessmentTaskCategoryIdAsync();
        if (!onsiteTaskCategoryId.HasValue)
        {
            return new PagedResultDto<OnsiteAssessmentTaskDto>(0, new List<OnsiteAssessmentTaskDto>());
        }

        _logger.LogWarning(
            "OnsiteAssessment GetList called: CurrentUserId={CurrentUserId}, EmployeeId={EmployeeId}, Skip={Skip}, Take={Take}",
            CurrentUser.Id,
            currentEmployeeId,
            input.SkipCount,
            input.MaxResultCount
        );
        var workTaskQuery = await WorkTaskRepository.GetQueryableAsync();
        var visibleStatuses = new[]
        {
            WorkTaskStatus.New,
            WorkTaskStatus.InProgress,
            WorkTaskStatus.Completed,
            WorkTaskStatus.Accepted,
            WorkTaskStatus.WaitApprove,
            WorkTaskStatus.Approved
        };

        var assignedPairs = await AsyncExecuter.ToListAsync(
            workTaskQuery
                .Where(wt =>
                    wt.BusinessCode == "CLAIM_ONSITE_ASSESSMENT"
                    && wt.TaskCategoryId == onsiteTaskCategoryId.Value
                    && (!input.WorkTaskStatus.HasValue || wt.Status == input.WorkTaskStatus.Value)
                    && (visibleStatuses.Contains(wt.Status)
                        || (wt.Status == WorkTaskStatus.Rejected && wt.ReporterId == currentEmployeeId))
                    && (wt.AssigneeId == currentEmployeeId || wt.ReporterId == currentEmployeeId))
                .Select(wt => new
            {
                wt.Id,
                AdjustAtLocationId = wt.BusinessKey,
                wt.Status,
                wt.ReporterId,
                wt.AssigneeId,
                wt.CreationTime
            }));

        if (assignedPairs.Count == 0)
        {
            return new PagedResultDto<OnsiteAssessmentTaskDto>(0, new List<OnsiteAssessmentTaskDto>());
        }

        var adjustIds = assignedPairs.Select(x => x.AdjustAtLocationId).Distinct().ToList();
        var adjustQuery = await ClaimAdjustAtLocationRepository.GetQueryableAsync();
        var adjusts = adjustIds.Count == 0
            ? new List<ClaimAdjustLink>()
            : await AsyncExecuter.ToListAsync(
                adjustQuery
                    .Where(a => adjustIds.Contains(a.Id))
                    .Select(a => new ClaimAdjustLink { Id = a.Id, ClaimId = a.ClaimId }));
        var claimIdByAdjustId = adjusts.ToDictionary(x => x.Id, x => x.ClaimId);

        var taskContexts = assignedPairs
            .Select(x =>
            {
                var hasAdjust = claimIdByAdjustId.TryGetValue(x.AdjustAtLocationId, out var claimId);
                return new OnsiteAssessmentTaskContext
                {
                    Id = x.Id,
                    ClaimId = hasAdjust ? claimId : x.AdjustAtLocationId,
                    ClaimAdjustAtLocationId = hasAdjust ? x.AdjustAtLocationId : Guid.Empty,
                    Status = x.Status,
                    ReporterId = x.ReporterId,
                    AssigneeId = x.AssigneeId,
                    CreationTime = x.CreationTime
                };
            })
            .ToList();

        var blockedReassignAdjustIds = await GetBlockedOnsiteReassignAdjustIdsAsync(adjustIds);

        var claimIds = taskContexts.Select(x => x.ClaimId).Distinct().ToList();

        var claimQuery = await ClaimRepository.GetQueryableAsync();
        claimQuery = claimQuery
            .Include(c => c.Lob)
            .Include(c => c.Insurer)
            .Include(c => c.OpenEmployee)
            .Where(c => claimIds.Contains(c.Id));

        claimQuery = await ApplyClaimFiltersAsync(claimQuery, input);

        var filteredClaims = await AsyncExecuter.ToListAsync(claimQuery);
        var claimById = filteredClaims.ToDictionary(c => c.Id, c => c);
        var filteredClaimIdSet = filteredClaims.Select(c => c.Id).ToHashSet();

        var filteredTasks = taskContexts
            .Where(wt => filteredClaimIdSet.Contains(wt.ClaimId)
                && (wt.Status != WorkTaskStatus.Rejected || !blockedReassignAdjustIds.Contains(wt.ClaimAdjustAtLocationId)))
            .ToList();

        var totalCount = filteredTasks.Count;

        var sortOrder = string.IsNullOrWhiteSpace(input.Sorting)
            ? "creationTime desc"
            : input.Sorting;

        var skip = input.SkipCount;
        var take = input.MaxResultCount > 0 ? input.MaxResultCount : 10;
        var pageTasks = ApplyTaskSorting(filteredTasks, claimById, sortOrder)
            .Skip(skip)
            .Take(take)
            .ToList();

        var pageClaimIds = pageTasks.Select(wt => wt.ClaimId).Distinct().ToHashSet();

        var incidents = await AsyncExecuter.ToListAsync(
            (await ClaimIncidentRepository.GetQueryableAsync()).Where(ci => pageClaimIds.Contains(ci.IncidentId)));
        var incidentByClaimId = incidents.ToDictionary(i => i.IncidentId, i => i);

        var incidentIds = incidents.Select(i => i.Id).ToList();
        var riskMotors = await AsyncExecuter.ToListAsync(
            (await ClaimIncidentRiskMotorRepository.GetQueryableAsync())
                .Where(rm => incidentIds.Contains(rm.IncidentObjectId ?? Guid.Empty)));
        var riskMotorByIncidentId = riskMotors
            .GroupBy(r => r.IncidentObjectId ?? Guid.Empty)
            .ToDictionary(g => g.Key, g => g.First());

        var employeeIds = pageTasks
            .SelectMany(t => new[] { t.ReporterId, t.AssigneeId ?? Guid.Empty })
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        var employees = await AsyncExecuter.ToListAsync(
            (await EmployeeRepository.GetQueryableAsync()).Where(e => employeeIds.Contains(e.Id)));
        var employeeNameById = employees.ToDictionary(e => e.Id, e => e.FullName);

        var items = new List<OnsiteAssessmentTaskDto>();
        foreach (var wt in pageTasks)
        {
            if (!claimById.TryGetValue(wt.ClaimId, out var claim))
            {
                continue;
            }

            incidentByClaimId.TryGetValue(claim.Id, out var incident);
            ClaimIncidentRiskMotor? riskMotor = null;
            if (incident != null)
            {
                riskMotorByIncidentId.TryGetValue(incident.Id, out riskMotor);
            }

            var reporterName = employeeNameById.GetValueOrDefault(wt.ReporterId);
            var assigneeName = wt.AssigneeId.HasValue ? employeeNameById.GetValueOrDefault(wt.AssigneeId.Value) : null;
            var isReporter = wt.ReporterId == currentEmployeeId;
            var isAssignee = wt.AssigneeId == currentEmployeeId;
            var canView = true;
            var canAccept = isAssignee && wt.Status == WorkTaskStatus.New;
            var canProcess = isAssignee && wt.Status == WorkTaskStatus.InProgress;
            var canComplete = isAssignee && wt.Status == WorkTaskStatus.InProgress;
            var canCancel = isReporter && wt.Status != WorkTaskStatus.Cancelled;
            var canReassign = isReporter
                && IsOnsiteAssessmentReassignableStatus(wt.Status)
                && !blockedReassignAdjustIds.Contains(wt.ClaimAdjustAtLocationId);

            items.Add(new OnsiteAssessmentTaskDto
            {
                Id = wt.Id,
                ClaimId = claim.Id,
                ClaimAdjustAtLocationId = wt.ClaimAdjustAtLocationId,
                Code = claim.Code,
                InsurerName = claim.Insurer?.Name,
                InsurerCode = claim.Insurer?.Code,
                LobName = claim.Lob?.Name,
                NotifierName = claim.NotifierName,
                NotifyDate = claim.NotifyDate,
                CarPlate = riskMotor?.CarPlate,
                ReporterName = reporterName,
                ReporterId = wt.ReporterId,
                AssigneeName = assigneeName,
                AssigneeId = wt.AssigneeId,
                ProcessClaimType = claim.ProcessClaimType,
                WorkTaskStatus = wt.Status,
                CanView = canView,
                CanAccept = canAccept,
                CanProcess = canProcess,
                CanComplete = canComplete,
                CanCancel = canCancel,
                IsReporter = isReporter,
                IsAssignee = isAssignee,
                CanReassign = canReassign
            });
        }

        return new PagedResultDto<OnsiteAssessmentTaskDto>(totalCount, items);
    }

    private async Task<OnsiteAssessmentDetailDto> BuildDetailDtoAsync(
        Guid claimId,
        WorkTask? workTask,
        Guid? adjustAtLocationId = null,
        Guid? currentEmployeeId = null)
    {
        var assessorName = workTask?.AssigneeId.HasValue == true
            ? (await EmployeeRepository.FirstOrDefaultAsync(e => e.Id == workTask.AssigneeId.Value))?.FullName
            : null;

        var assessorDeptName = workTask?.AssigneeDepartmentId.HasValue == true
            ? (await DepartmentRepository.FirstOrDefaultAsync(d => d.Id == workTask.AssigneeDepartmentId.Value))?.Name
            : null;

        var incident = await ClaimIncidentRepository.FirstOrDefaultAsync(x => x.IncidentId == claimId);
        var riskMotor = incident == null
            ? null
            : await ClaimIncidentRiskMotorRepository.FirstOrDefaultAsync(x => x.IncidentObjectId == incident.Id);

        var adjust = adjustAtLocationId.HasValue && adjustAtLocationId.Value != Guid.Empty
            ? await ClaimAdjustAtLocationRepository.FirstOrDefaultAsync(x => x.Id == adjustAtLocationId.Value)
            : workTask != null
                ? (await ResolveWorkTaskContextAsync(workTask)).Adjust
            : await GetLatestClaimAdjustAtLocationAsync(claimId);
        var garageName = adjust?.GarageId != null
            ? (await PartnerRepository.FirstOrDefaultAsync(x => x.Id == adjust.GarageId.Value))?.Name
            : null;

        var lossPositions = new List<string>();
        if (!string.IsNullOrWhiteSpace(adjust?.LossPosition))
        {
            lossPositions = adjust.LossPosition
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();
        }

        var claimDocQuery = await ClaimDocumentRepository.GetQueryableAsync();
        claimDocQuery = claimDocQuery.Where(x => x.ClaimId == claimId);
        if (adjust != null)
        {
            claimDocQuery = claimDocQuery.Where(x => x.AdjustAtLocationId == adjust.Id);
        }

        var claimDocs = await AsyncExecuter.ToListAsync(claimDocQuery.OrderBy(x => x.CreationTime));
        var documentIds = claimDocs
            .Where(x => x.DocumentId.HasValue)
            .Select(x => x.DocumentId!.Value)
            .Distinct()
            .ToList();

        var resDocById = new Dictionary<Guid, ResDocument>();
        var typeById = new Dictionary<Guid, ResDocumentType>();
        var claimDocumentTypeIds = claimDocs
            .Where(x => x.DocumentTypeId.HasValue)
            .Select(x => x.DocumentTypeId!.Value)
            .Distinct()
            .ToList();
        if (documentIds.Count > 0)
        {
            var resDocs = await AsyncExecuter.ToListAsync(
                (await ResDocumentRepository.GetQueryableAsync()).Where(x => documentIds.Contains(x.Id)));
            resDocById = resDocs.ToDictionary(x => x.Id, x => x);

            claimDocumentTypeIds = claimDocumentTypeIds
                .Concat(resDocs.Select(x => x.DocumentTypeId))
                .Distinct()
                .ToList();
        }

        if (claimDocumentTypeIds.Count > 0)
        {
            var types = await AsyncExecuter.ToListAsync(
                (await ResDocumentTypeRepository.GetQueryableAsync()).Where(x => claimDocumentTypeIds.Contains(x.Id)));
            typeById = types.ToDictionary(x => x.Id, x => x);
        }

        var creatorUserIds = claimDocs
            .Where(x => x.CreatorId.HasValue)
            .Select(x => x.CreatorId!.Value)
            .Distinct()
            .ToList();
        var uploaderByUserId = new Dictionary<Guid, string>();
        if (creatorUserIds.Count > 0)
        {
            var employees = await AsyncExecuter.ToListAsync(
                (await EmployeeRepository.GetQueryableAsync())
                    .Where(x => x.UserId.HasValue && creatorUserIds.Contains(x.UserId.Value)));
            uploaderByUserId = employees
                .Where(x => x.UserId.HasValue)
                .GroupBy(x => x.UserId!.Value)
                .ToDictionary(g => g.Key, g => g.First().FullName);
        }

        var images = new List<OnsiteAssessmentImageDto>();
        var documents = new List<OnsiteAssessmentDocumentDto>();
        foreach (var cd in claimDocs)
        {
            if (!cd.DocumentId.HasValue)
            {
                var metadataDocType = cd.DocumentTypeId.HasValue && typeById.TryGetValue(cd.DocumentTypeId.Value, out var matchedMetadataDocType)
                    ? matchedMetadataDocType
                    : null;
                if (!IsOnsiteProfileDocumentType(metadataDocType))
                {
                    continue;
                }

                documents.Add(new OnsiteAssessmentDocumentDto
                {
                    ClaimDocumentId = cd.Id,
                    DocumentId = null,
                    DocumentTypeId = metadataDocType?.Id,
                    DocumentTypeName = metadataDocType?.Name,
                    FileName = null,
                    Url = null,
                    ThumbnailUrl = null,
                    Complete = cd.Complete,
                    IsCopy = cd.IsCopy,
                    Note = cd.Note,
                    IssueDate = cd.IssueDate
                });
                continue;
            }
            if (!resDocById.TryGetValue(cd.DocumentId.Value, out var rd)) continue;

            typeById.TryGetValue(rd.DocumentTypeId, out var docType);
            var docTypeCode = (docType?.Code ?? string.Empty).Trim();
            var isImage = string.Equals(docTypeCode, "CAR_ASSESSMENT_IMAGE", StringComparison.OrdinalIgnoreCase)
                || string.Equals(docType?.DocumentGroupCode, "CAR_ASSESSMENT_IMAGE", StringComparison.OrdinalIgnoreCase)
                || string.Equals(rd.GroupCode, "CAR_ASSESSMENT_IMAGE", StringComparison.OrdinalIgnoreCase);
            var isOnsiteProfile = IsOnsiteProfileDocumentType(docType);

            if (isImage)
            {
                images.Add(new OnsiteAssessmentImageDto
                {
                    ClaimDocumentId = cd.Id,
                    DocumentId = cd.DocumentId,
                    DocumentTypeId = rd.DocumentTypeId,
                    DocumentTypeName = docType?.Name,
                    FileName = rd.FileName,
                    Url = rd.Url,
                    ThumbnailUrl = rd.ThumbnailUrl,
                    UploadedAt = cd.CreationTime,
                    UploaderName = cd.CreatorId.HasValue
                        ? uploaderByUserId.GetValueOrDefault(cd.CreatorId.Value)
                        : null
                });
                continue;
            }

            if (!isOnsiteProfile)
            {
                continue;
            }

            documents.Add(new OnsiteAssessmentDocumentDto
            {
                ClaimDocumentId = cd.Id,
                DocumentId = cd.DocumentId,
                DocumentTypeId = rd.DocumentTypeId,
                DocumentTypeName = docType?.Name,
                FileName = rd.FileName,
                Url = rd.Url,
                ThumbnailUrl = rd.ThumbnailUrl,
                Complete = cd.Complete,
                IsCopy = cd.IsCopy,
                Note = cd.Note,
                IssueDate = cd.IssueDate
            });
        }

        return new OnsiteAssessmentDetailDto
        {
            WorkTaskId = workTask?.Id ?? Guid.Empty,
            WorkTaskStatus = workTask?.Status ?? WorkTaskStatus.InProgress,
            ClaimId = claimId,
            IsReporter = currentEmployeeId.HasValue && workTask?.ReporterId == currentEmployeeId.Value,
            AssessorDeptName = assessorDeptName,
            AssessorName = assessorName,
            StartDate = workTask?.StartDate,
            EndDate = workTask?.EndDate,
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
            LossPositions = lossPositions,
            HasLossThirdParty = string.Equals(adjust?.HasLossThirdParty, "Y", StringComparison.OrdinalIgnoreCase),
            WitnessTestimony = adjust?.WitnessTestimony,
            CauseDescription = adjust?.CauseDescription,
            Description = adjust?.Description,
            LocationDescription = adjust?.LocationDescription,
            DamageDescription = adjust?.DamageDescription,
            PartiesInvolvedDescription = adjust?.PartiesInvolvedDescription,
            AddressPlan = adjust?.AddressPlan,
            CustomerRecommendation = adjust?.CustomerRecommendation,
            OtherDescription = adjust?.OtherDescription,
            GarageId = adjust?.GarageId,
            GarageName = garageName,
            IssueDate = adjust?.IssueDate,
            Images = images,
            Documents = documents
        };
    }

    public virtual async Task<IRemoteStreamContent> ExportAsync(GetOnsiteAssessmentTasksInput input)
    {
        input.MaxResultCount = 10000;
        input.SkipCount = 0;

        var list = await GetListAsync(input);

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("OnsiteAssessmentTasks");

        var headers = new[]
        {
            "Mã yêu cầu",
            "Bảo hiểm gốc",
            "Nghiệp vụ BH",
            "Người thông báo",
            "Ngày thông báo",
            "Biển số xe",
            "Người giao GĐHT",
            "Người GĐHT",
            "Bên xử lý",
            "Trạng thái"
        };

        for (var i = 0; i < headers.Length; i++)
        {
            ws.Cell(1, i + 1).Value = headers[i];
            ws.Cell(1, i + 1).Style.Font.Bold = true;
        }

        var row = 2;
        foreach (var t in list.Items)
        {
            ws.Cell(row, 1).Value = t.Code ?? string.Empty;
            ws.Cell(row, 2).Value = t.InsurerCode ?? string.Empty;
            ws.Cell(row, 3).Value = t.LobName ?? string.Empty;
            ws.Cell(row, 4).Value = t.NotifierName ?? string.Empty;
            ws.Cell(row, 5).Value = t.NotifyDate?.ToString("dd/MM/yyyy HH:mm:ss") ?? string.Empty;
            ws.Cell(row, 6).Value = t.CarPlate ?? string.Empty;
            ws.Cell(row, 7).Value = t.ReporterName ?? string.Empty;
            ws.Cell(row, 8).Value = t.AssigneeName ?? string.Empty;
            ws.Cell(row, 9).Value = t.ProcessClaimType == ProcessClaimType.Own ? "Tự xử lý" : "Bảo hiểm gốc xử lý";
            ws.Cell(row, 10).Value = t.WorkTaskStatus.ToString();
            row++;
        }

        ws.Columns().AdjustToContents();

        var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        var fileName = $"onsite-assessment-tasks_{Clock.Now:yyyyMMdd}.xlsx";
        return new RemoteStreamContent(stream, fileName, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
    }

    public virtual async Task<List<OnsiteAssessmentCreateRequestDto>> GetCreateRequestListAsync()
    {
        var currentEmployeeId = await GetCurrentEmployeeIdAsync();
        var claimAssignTaskCategoryId = await ResTaskCategoryRepository.FirstOrDefaultAsync(c => c.Code == "CLAIM_ASSIGN_TASK");
        if (claimAssignTaskCategoryId == null)
        {
            return new List<OnsiteAssessmentCreateRequestDto>();
        }

        var workTaskQuery = await WorkTaskRepository.GetQueryableAsync();
        var claimQuery = await ClaimRepository.GetQueryableAsync();
        var incidentQuery = await ClaimIncidentRepository.GetQueryableAsync();
        var riskMotorQuery = await ClaimIncidentRiskMotorRepository.GetQueryableAsync();
        var adjustQuery = await ClaimAdjustAtLocationRepository.GetQueryableAsync();

        var claimAssignClaimIds = await AsyncExecuter.ToListAsync(
            workTaskQuery
                .Where(w =>
                    w.BusinessCode == "CLAIM_ASSIGN"
                    && w.TaskCategoryId == claimAssignTaskCategoryId.Id
                    && w.AssigneeId == currentEmployeeId
                    && w.Status == WorkTaskStatus.InProgress)
                .Select(w => w.BusinessKey)
        );

        var candidateClaimIds = claimAssignClaimIds.Distinct().ToList();
        if (candidateClaimIds.Count == 0)
        {
            return new List<OnsiteAssessmentCreateRequestDto>();
        }

        var incidents = await AsyncExecuter.ToListAsync(
            incidentQuery
                .Where(i => candidateClaimIds.Contains(i.IncidentId) && i.OnLocation == "Y")
                .Select(i => new { i.Id, ClaimId = i.IncidentId })
        );
        var onsiteClaimIds = incidents.Select(x => x.ClaimId).Distinct().ToList();
        if (onsiteClaimIds.Count == 0)
        {
            return new List<OnsiteAssessmentCreateRequestDto>();
        }

        var adjusts = await AsyncExecuter.ToListAsync(
            adjustQuery
                .Where(a => onsiteClaimIds.Contains(a.ClaimId))
                .Select(a => new { a.Id, a.ClaimId })
        );
        var claimIdByAdjustId = adjusts
            .GroupBy(x => x.Id)
            .ToDictionary(g => g.Key, g => g.First().ClaimId);
        var onsiteAdjustIds = adjusts.Select(x => x.Id).Distinct().ToList();

        var existingOnsiteTaskClaimIds = await AsyncExecuter.ToListAsync(
            workTaskQuery
                .Where(w =>
                    w.BusinessCode == "CLAIM_ONSITE_ASSESSMENT"
                    && onsiteAdjustIds.Contains(w.BusinessKey)
                    && w.Status != WorkTaskStatus.Cancelled)
                .Select(w => w.BusinessKey)
        );
        var excludedClaimIds = existingOnsiteTaskClaimIds
            .Select(x => claimIdByAdjustId.TryGetValue(x, out var claimId) ? claimId : x)
            .Distinct()
            .ToHashSet();

        var claims = await AsyncExecuter.ToListAsync(
            claimQuery
                .Where(c =>
                    onsiteClaimIds.Contains(c.Id)
                    && c.Status == ClaimStatus.InProgress
                    && !excludedClaimIds.Contains(c.Id))
                .Select(c => new { c.Id, c.Code, c.NotifyDate, c.NotifierName })
        );

        if (claims.Count == 0)
        {
            return new List<OnsiteAssessmentCreateRequestDto>();
        }

        var incidentByClaimId = incidents
            .Where(x => claims.Any(c => c.Id == x.ClaimId))
            .GroupBy(x => x.ClaimId)
            .ToDictionary(g => g.Key, g => g.First().Id);
        var incidentIds = incidents.Select(x => x.Id).ToList();

        var riskMotors = await AsyncExecuter.ToListAsync(
            riskMotorQuery
                .Where(r => r.IncidentObjectId.HasValue && incidentIds.Contains(r.IncidentObjectId.Value))
                .Select(r => new { r.IncidentObjectId, r.CarPlate })
        );
        var plateByIncidentId = riskMotors
            .Where(x => x.IncidentObjectId.HasValue)
            .GroupBy(x => x.IncidentObjectId!.Value)
            .ToDictionary(g => g.Key, g => g.First().CarPlate);

        var result = claims
            .OrderByDescending(c => c.NotifyDate)
            .Select(c =>
            {
                string? carPlate = null;
                if (incidentByClaimId.TryGetValue(c.Id, out var incidentId))
                {
                    plateByIncidentId.TryGetValue(incidentId, out carPlate);
                }

                return new OnsiteAssessmentCreateRequestDto
                {
                    ClaimId = c.Id,
                    ClaimCode = c.Code,
                    NotifyDate = c.NotifyDate,
                    NotifierName = c.NotifierName,
                    CarPlate = carPlate
                };
            })
            .ToList();

        return result;
    }

    public virtual async Task<OnsiteAssessmentCreateResultDto> CreateOnsiteAssessmentAsync(CreateOnsiteAssessmentRequestInput input)
    {
        if (input.ClaimId == Guid.Empty)
        {
            throw new Volo.Abp.UserFriendlyException("Yêu cầu bồi thường là bắt buộc.");
        }
        if (!input.AssigneeOrganizationId.HasValue || input.AssigneeOrganizationId == Guid.Empty)
        {
            throw new Volo.Abp.UserFriendlyException("Đơn vị giám định là bắt buộc.");
        }
        if (!input.AssigneeId.HasValue || input.AssigneeId == Guid.Empty)
        {
            throw new Volo.Abp.UserFriendlyException("Người giám định là bắt buộc.");
        }
        if (!input.StartDate.HasValue)
        {
            throw new Volo.Abp.UserFriendlyException("Ngày giám định là bắt buộc.");
        }
        if (!input.EndDate.HasValue)
        {
            throw new Volo.Abp.UserFriendlyException("Ngày dự kiến hoàn thành là bắt buộc.");
        }
        if (input.EndDate.Value < input.StartDate.Value)
        {
            throw new Volo.Abp.UserFriendlyException("Ngày dự kiến hoàn thành phải lớn hơn hoặc bằng ngày giám định.");
        }

        var currentEmployeeId = await GetCurrentEmployeeIdAsync();
        var claim = await ClaimRepository.FirstOrDefaultAsync(c => c.Id == input.ClaimId);
        if (claim == null)
        {
            throw new Volo.Abp.UserFriendlyException(L["ClaimTask:ClaimNotFound"].Value);
        }

        var claimAssignTaskCategory = await ResTaskCategoryRepository.FirstOrDefaultAsync(c => c.Code == "CLAIM_ASSIGN_TASK");
        if (claimAssignTaskCategory == null)
        {
            throw new Volo.Abp.UserFriendlyException(L["ClaimTask:InvalidStatus"].Value);
        }

        var claimAssignTaskQuery = await WorkTaskRepository.GetQueryableAsync();
        var canCreate = await AsyncExecuter.AnyAsync(
            claimAssignTaskQuery.Where(w =>
                w.BusinessCode == "CLAIM_ASSIGN"
                && w.BusinessKey == input.ClaimId
                && w.TaskCategoryId == claimAssignTaskCategory.Id
                && w.AssigneeId == currentEmployeeId
                && w.Status == WorkTaskStatus.InProgress));
        if (!canCreate)
        {
            throw new Volo.Abp.UserFriendlyException(L["ClaimTask:NotAuthorized"].Value);
        }

        var adjust = await _onsiteAssessmentAssignmentService.CreateAsync(
            input.ClaimId,
            input.AssigneeOrganizationId.Value,
            input.AssigneeId.Value,
            input.StartDate.Value,
            input.EndDate.Value);

        var onsiteTask = await WorkTaskRepository.FirstOrDefaultAsync(w =>
            w.BusinessCode == "CLAIM_ONSITE_ASSESSMENT"
            && w.BusinessKey == adjust.Id
            && w.AssigneeId == input.AssigneeId.Value);

        return new OnsiteAssessmentCreateResultDto
        {
            ClaimId = claim.Id,
            WorkTaskId = onsiteTask?.Id ?? Guid.Empty
        };
    }

    public virtual async Task CancelAsync(Guid workTaskId)
    {
        var currentEmployeeId = await GetCurrentEmployeeIdAsync();
        var categoryId = await GetOnsiteAssessmentTaskCategoryIdAsync();
        if (categoryId == null)
        {
            throw new Volo.Abp.UserFriendlyException(L["ClaimTask:InvalidStatus"].Value);
        }

        var workTask = await WorkTaskRepository.GetAsync(workTaskId);
        if (!IsOnsiteAssessmentBusinessCode(workTask.BusinessCode) || workTask.TaskCategoryId != categoryId.Value)
        {
            throw new Volo.Abp.UserFriendlyException(L["ClaimTask:NotAuthorized"].Value);
        }

        if (workTask.ReporterId != currentEmployeeId)
        {
            throw new Volo.Abp.UserFriendlyException(L["ClaimTask:NotAuthorized"].Value);
        }

        if (workTask.Status == WorkTaskStatus.Cancelled || workTask.Status == WorkTaskStatus.Completed)
        {
            throw new Volo.Abp.UserFriendlyException(L["ClaimTask:InvalidStatus"].Value);
        }

        workTask.UpdateStatus(WorkTaskStatus.Cancelled);
        workTask.UpdateActualDates(workTask.ActualStartDate, Clock.Now);
        await WorkTaskRepository.UpdateAsync(workTask);

        var context = await ResolveWorkTaskContextAsync(workTask);
        var adjust = context.Adjust;
        if (adjust != null)
        {
            adjust.UpdateStatus(ClaimAdjustAtLocationStatus.Cancel);
            await ClaimAdjustAtLocationRepository.UpdateAsync(adjust);
        }

        if (workTask.WorkInstanceId != null)
        {
            var workInstance = await _workInstanceRepository.GetAsync(workTask.WorkInstanceId.Value);
            var eventName = workTask.EventName ?? workInstance.WorkflowInstanceId;
            await _elsaWorkflowService.TriggerApprovalAsync(eventName, workInstance.WorkflowInstanceId, "cancel", null);
        }
    }

    public virtual async Task SaveOnsiteAssessmentAsync(Guid workTaskId, SaveOnsiteAssessmentInput input)
    {
        var currentEmployeeId = await GetCurrentEmployeeIdAsync();
        ValidateSaveInput(input);

        var workTask = await WorkTaskRepository.GetAsync(workTaskId);
        if (!IsOnsiteAssessmentBusinessCode(workTask.BusinessCode))
        {
            throw new Volo.Abp.UserFriendlyException(L["ClaimTask:NotAuthorized"].Value);
        }

        if (workTask.AssigneeId != currentEmployeeId && workTask.ReporterId != currentEmployeeId)
        {
            throw new Volo.Abp.UserFriendlyException(L["ClaimTask:NotAuthorized"].Value);
        }

        if (workTask.Status == WorkTaskStatus.New)
        {
            workTask.UpdateStatus(WorkTaskStatus.InProgress);
            workTask.UpdateActualDates(workTask.ActualStartDate ?? Clock.Now, workTask.ActualEndDate);
            await WorkTaskRepository.UpdateAsync(workTask);
        }
        else if (workTask.Status != WorkTaskStatus.InProgress && workTask.Status != WorkTaskStatus.Accepted)
        {
            throw new Volo.Abp.UserFriendlyException(L["ClaimTask:InvalidStatus"].Value);
        }

        var context = await ResolveWorkTaskContextAsync(workTask);
        var claimId = context.ClaimId;
        var incident = await ClaimIncidentRepository.FirstOrDefaultAsync(x => x.IncidentId == claimId);
        if (incident == null)
        {
            throw new Volo.Abp.UserFriendlyException("Không tìm thấy thông tin tổn thất của yêu cầu.");
        }

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

        var adjust = context.Adjust;
        if (adjust == null)
        {
            adjust = new ClaimAdjustAtLocation(
                GuidGenerator.Create(),
                claimId,
                workTask.AssigneeId ?? currentEmployeeId,
                startDate: workTask.StartDate,
                endDate: workTask.EndDate,
                status: ClaimAdjustAtLocationStatus.InProgress);
            await ClaimAdjustAtLocationRepository.InsertAsync(adjust, autoSave: true);
        }
        else
        {
            adjust.UpdateAssignment(
                workTask.AssigneeId ?? currentEmployeeId,
                workTask.StartDate,
                workTask.EndDate,
                status: null);
        }

        adjust.UpdateAssessmentData(
            string.Join(",", input.LossPositions ?? new List<string>()),
            input.HasLossThirdParty ? "Y" : "N",
            NormalizeOptionalText(input.WitnessTestimony),
            NormalizeOptionalText(input.CauseDescription),
            NormalizeOptionalText(input.Description),
            NormalizeOptionalText(input.LocationDescription),
            NormalizeOptionalText(input.DamageDescription),
            NormalizeOptionalText(input.PartiesInvolvedDescription),
            NormalizeOptionalText(input.AddressPlan),
            NormalizeOptionalText(input.CustomerRecommendation),
            NormalizeOptionalText(input.OtherDescription, false),
            input.GarageId,
            input.IssueDate
        );
        if (adjust.Status == null || adjust.Status == ClaimAdjustAtLocationStatus.New)
        {
            adjust.UpdateStatus(ClaimAdjustAtLocationStatus.InProgress);
        }
        await ClaimAdjustAtLocationRepository.UpdateAsync(adjust);

        await UpsertOnsiteDocumentsAsync(claimId, adjust.Id, input);
    }

    public virtual async Task SaveAndAssignOnsiteAssessmentAsync(Guid workTaskId, SaveOnsiteAssessmentInput input)
    {
        await SaveOnsiteAssessmentAsync(workTaskId, input);

        var workTask = await WorkTaskRepository.GetAsync(workTaskId);
        if (workTask.Status == WorkTaskStatus.Cancelled || workTask.Status == WorkTaskStatus.Completed)
        {
            throw new Volo.Abp.UserFriendlyException(L["ClaimTask:InvalidStatus"].Value);
        }

        if (workTask.Status != WorkTaskStatus.Completed)
        {
            workTask.UpdateStatus(WorkTaskStatus.Completed);
            workTask.UpdateActualDates(workTask.ActualStartDate ?? Clock.Now, Clock.Now);
            await WorkTaskRepository.UpdateAsync(workTask);
        }

        var context = await ResolveWorkTaskContextAsync(workTask);
        var adjust = context.Adjust;
        if (adjust != null)
        {
            adjust.MarkDone(Clock.Now);
            await ClaimAdjustAtLocationRepository.UpdateAsync(adjust);
        }

        if (workTask.WorkInstanceId != null)
        {
            var workInstance = await _workInstanceRepository.GetAsync(workTask.WorkInstanceId.Value);
            var eventName = workTask.EventName ?? workInstance.WorkflowInstanceId;
            await _elsaWorkflowService.TriggerApprovalAsync(eventName, workInstance.WorkflowInstanceId, "complete", null);
        }
    }

    public virtual async Task RemoveOnsiteProfileFileAsync(Guid workTaskId, Guid documentId)
    {
        var currentEmployeeId = await GetCurrentEmployeeIdAsync();
        var workTask = await WorkTaskRepository.GetAsync(workTaskId);
        if (!IsOnsiteAssessmentBusinessCode(workTask.BusinessCode))
        {
            throw new Volo.Abp.UserFriendlyException(L["ClaimTask:NotAuthorized"].Value);
        }

        if (workTask.AssigneeId != currentEmployeeId && workTask.ReporterId != currentEmployeeId)
        {
            throw new Volo.Abp.UserFriendlyException(L["ClaimTask:NotAuthorized"].Value);
        }

        var context = await ResolveWorkTaskContextAsync(workTask);
        var claimId = context.ClaimId;
        var adjust = context.Adjust;
        if (adjust == null)
        {
            return;
        }

        var resDoc = await ResDocumentRepository.FirstOrDefaultAsync(x => x.Id == documentId);
        if (resDoc != null)
        {
            var docType = await ResDocumentTypeRepository.FirstOrDefaultAsync(x => x.Id == resDoc.DocumentTypeId);
            var docTypeCode = (docType?.Code ?? string.Empty).Trim();
            var isImage = docTypeCode.Equals("CAR_ASSESSMENT_IMAGE", StringComparison.OrdinalIgnoreCase)
                || string.Equals(docType?.DocumentGroupCode, "CAR_ASSESSMENT_IMAGE", StringComparison.OrdinalIgnoreCase)
                || string.Equals(resDoc.GroupCode, "CAR_ASSESSMENT_IMAGE", StringComparison.OrdinalIgnoreCase);
            if (isImage)
            {
                throw new Volo.Abp.UserFriendlyException("Không thể xoá ảnh hiện trường bằng chức năng xoá hồ sơ giấy tờ.");
            }
        }

        var claimDocs = await AsyncExecuter.ToListAsync(
            (await ClaimDocumentRepository.GetQueryableAsync())
            .Where(x =>
                x.ClaimId == claimId &&
                x.DocumentId == documentId &&
                x.AdjustAtLocationId == adjust.Id));

        if (claimDocs.Count == 0)
        {
            return;
        }

        foreach (var claimDoc in claimDocs)
        {
            await ClaimDocumentRepository.DeleteAsync(claimDoc);
        }

        var hasOtherReferences = await AsyncExecuter.AnyAsync(
            (await ClaimDocumentRepository.GetQueryableAsync())
            .Where(x => x.DocumentId == documentId));

        if (!hasOtherReferences && resDoc != null)
        {
            await ResDocumentRepository.DeleteAsync(resDoc);
        }
    }

    public virtual async Task AcceptAsync(Guid workTaskId)
    {
        var currentEmployeeId = await GetCurrentEmployeeIdAsync();
        var workTask = await WorkTaskRepository.GetAsync(workTaskId);
        if (!IsOnsiteAssessmentBusinessCode(workTask.BusinessCode) || workTask.AssigneeId != currentEmployeeId)
        {
            throw new Volo.Abp.UserFriendlyException(L["ClaimTask:NotAuthorized"].Value);
        }

        if (workTask.Status != WorkTaskStatus.New)
        {
            throw new Volo.Abp.UserFriendlyException(L["ClaimTask:InvalidStatus"].Value);
        }

        workTask.UpdateStatus(WorkTaskStatus.InProgress);
        workTask.UpdateActualDates(Clock.Now, null);
        await WorkTaskRepository.UpdateAsync(workTask);

        if (workTask.WorkInstanceId != null)
        {
            var workInstance = await _workInstanceRepository.GetAsync(workTask.WorkInstanceId.Value);
            var eventName = workTask.EventName ?? workInstance.WorkflowInstanceId;
            await _elsaWorkflowService.TriggerApprovalAsync(eventName, workInstance.WorkflowInstanceId, "approve", null);
        }
    }

    public virtual async Task RejectAsync(Guid workTaskId, OnsiteRejectTaskInput input)
    {
        var currentEmployeeId = await GetCurrentEmployeeIdAsync();
        var workTask = await WorkTaskRepository.GetAsync(workTaskId);
        if (!IsOnsiteAssessmentBusinessCode(workTask.BusinessCode) || workTask.AssigneeId != currentEmployeeId)
        {
            throw new Volo.Abp.UserFriendlyException(L["ClaimTask:NotAuthorized"].Value);
        }

        if (workTask.Status != WorkTaskStatus.New)
        {
            throw new Volo.Abp.UserFriendlyException(L["ClaimTask:InvalidStatus"].Value);
        }

        workTask.UpdateRejectionReason(input.ReasonId, input.ReasonDescription?.Trim());
        workTask.UpdateStatus(WorkTaskStatus.Rejected);
        workTask.UpdateActualDates(workTask.ActualStartDate, Clock.Now);
        await WorkTaskRepository.UpdateAsync(workTask);

        if (workTask.WorkInstanceId != null)
        {
            var workInstance = await _workInstanceRepository.GetAsync(workTask.WorkInstanceId.Value);
            var eventName = workTask.EventName ?? workInstance.WorkflowInstanceId;
            await _elsaWorkflowService.TriggerApprovalAsync(eventName, workInstance.WorkflowInstanceId, "reject", null);
        }
    }

    public virtual async Task TransferAsync(Guid workTaskId, OnsiteTransferTaskInput input)
    {
        var currentEmployeeId = await GetCurrentEmployeeIdAsync();
        var workTask = await WorkTaskRepository.GetAsync(workTaskId);
        if (!IsOnsiteAssessmentBusinessCode(workTask.BusinessCode) || workTask.AssigneeId != currentEmployeeId)
        {
            throw new Volo.Abp.UserFriendlyException(L["ClaimTask:NotAuthorized"].Value);
        }

        if (workTask.Status != WorkTaskStatus.New)
        {
            throw new Volo.Abp.UserFriendlyException(L["ClaimTask:InvalidStatus"].Value);
        }
        if (input.AssigneeId == currentEmployeeId)
        {
            throw new Volo.Abp.UserFriendlyException("Người điều chuyển phải khác người đang xử lý.");
        }

        _ = await EmployeeRepository.GetAsync(input.AssigneeId);

        workTask.UpdateRejectionReason(input.ReasonId, input.ReasonDescription?.Trim());
        workTask.UpdateStatus(WorkTaskStatus.Return);
        workTask.UpdateActualDates(workTask.ActualStartDate, Clock.Now);
        await WorkTaskRepository.UpdateAsync(workTask);

        // Do not create the next onsite task here.
        // The workflow transition below is responsible for creating the new work task for the reassigned assignee.

        if (workTask.WorkInstanceId != null)
        {
            var workInstance = await _workInstanceRepository.GetAsync(workTask.WorkInstanceId.Value);
            var eventName = workTask.EventName ?? workInstance.WorkflowInstanceId;
            await _elsaWorkflowService.TriggerApprovalAsync(
                eventName,
                workInstance.WorkflowInstanceId,
                "next",
                input.AssigneeId.ToString()
            );
        }
    }

    public virtual async Task ReassignAsync(Guid workTaskId, ReassignOnsiteAssessmentInput input)
    {
        var currentEmployeeId = await GetCurrentEmployeeIdAsync();
        var workTask = await WorkTaskRepository.GetAsync(workTaskId);
        if (!IsActualOnsiteAssessmentBusinessCode(workTask.BusinessCode) || workTask.ReporterId != currentEmployeeId)
        {
            throw new Volo.Abp.UserFriendlyException(L["ClaimTask:NotAuthorized"].Value);
        }

        if (!IsOnsiteAssessmentReassignableStatus(workTask.Status))
        {
            throw new Volo.Abp.UserFriendlyException(L["ClaimTask:InvalidStatus"].Value);
        }

        if (await HasOtherActiveOnsiteTaskAsync(workTask.BusinessKey, workTask.Id))
        {
            throw new Volo.Abp.UserFriendlyException("Yêu cầu còn công việc giám định hiện trường chưa ở trạng thái từ chối hoặc trả lại, không thể giao lại.");
        }

        var assignee = await EmployeeRepository.FirstOrDefaultAsync(x => x.Id == input.AssigneeId);
        if (assignee == null)
        {
            throw new Volo.Abp.UserFriendlyException("Người giám định không hợp lệ.");
        }

        if (assignee.DepartmentId != input.AssigneeOrganizationId)
        {
            throw new Volo.Abp.UserFriendlyException("Người giám định không thuộc đơn vị đã chọn.");
        }

        if (input.EndDate < input.StartDate)
        {
            throw new Volo.Abp.UserFriendlyException(L["AssessmentStartDateMustBeBeforeOrEqualExpectedCompletionDate"].Value);
        }

        var adjust = await ClaimAdjustAtLocationRepository.FirstOrDefaultAsync(x => x.Id == workTask.BusinessKey);
        if (adjust == null)
        {
            throw new Volo.Abp.UserFriendlyException(L["ClaimTask:ClaimNotFound"].Value);
        }

        adjust.UpdateAssignment(input.AssigneeId, input.StartDate, input.EndDate, ClaimAdjustAtLocationStatus.New);
        await ClaimAdjustAtLocationRepository.UpdateAsync(adjust);

        await _elsaWorkflowService.InitOnsiteAssessmentWorkflowAsync(
            adjust.Id,
            input.AssigneeOrganizationId,
            input.AssigneeId,
            input.StartDate,
            input.EndDate);
    }

    private async Task<Guid> GetCurrentEmployeeIdAsync()
    {
        if (CurrentUser.Id == null)
        {
            throw new Volo.Abp.UserFriendlyException(L["UserNotAuthenticated"].Value);
        }

        var employee = await EmployeeRepository.FirstOrDefaultAsync(e => e.UserId == CurrentUser.Id.Value);
        if (employee == null)
        {
            throw new Volo.Abp.UserFriendlyException(L["EmployeeNotFoundForUser"].Value);
        }

        _logger.LogWarning(
            "OnsiteAssessment search mapping: CurrentUserId={CurrentUserId}, EmployeeId={EmployeeId}, EmployeeCode={EmployeeCode}",
            CurrentUser.Id,
            employee.Id,
            employee.Code
        );

        return employee.Id;
    }

    private static bool IsOnsiteAssessmentBusinessCode(string? businessCode)
    {
        if (string.IsNullOrWhiteSpace(businessCode))
        {
            return false;
        }

        return string.Equals(businessCode, "CLAIM_ONSITE_ASSESSMENT", StringComparison.OrdinalIgnoreCase)
               || string.Equals(businessCode, "claim_adjust_at_location", StringComparison.OrdinalIgnoreCase)
               || string.Equals(businessCode, "CLAIM_ASSIGN", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsActualOnsiteAssessmentBusinessCode(string? businessCode)
    {
        return string.Equals(businessCode, "CLAIM_ONSITE_ASSESSMENT", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsOnsiteAssessmentReassignableStatus(WorkTaskStatus status)
    {
        return status == WorkTaskStatus.Rejected
            || status == WorkTaskStatus.Return
            || status == WorkTaskStatus.Completed;
    }

    private async Task<bool> HasOtherActiveOnsiteTaskAsync(Guid adjustAtLocationId, Guid currentWorkTaskId)
    {
        var workTaskQuery = await WorkTaskRepository.GetQueryableAsync();
        var relatedTasks = await AsyncExecuter.ToListAsync(
            workTaskQuery.Where(wt =>
                wt.BusinessKey == adjustAtLocationId
                && wt.BusinessCode == "CLAIM_ONSITE_ASSESSMENT"));

        return relatedTasks.Any(wt =>
            wt.Id != currentWorkTaskId
            && wt.Status != WorkTaskStatus.Rejected
            && wt.Status != WorkTaskStatus.Return
            && wt.Status != WorkTaskStatus.Completed);
    }

    private async Task<HashSet<Guid>> GetBlockedOnsiteReassignAdjustIdsAsync(List<Guid> adjustAtLocationIds)
    {
        if (adjustAtLocationIds.Count == 0)
        {
            return new HashSet<Guid>();
        }

        var workTaskQuery = await WorkTaskRepository.GetQueryableAsync();
        var blockedIds = await AsyncExecuter.ToListAsync(
            workTaskQuery
                .Where(wt =>
                    adjustAtLocationIds.Contains(wt.BusinessKey)
                    && wt.BusinessCode == "CLAIM_ONSITE_ASSESSMENT"
                    && wt.Status != WorkTaskStatus.Rejected
                    && wt.Status != WorkTaskStatus.Return
                    && wt.Status != WorkTaskStatus.Completed)
                .Select(wt => wt.BusinessKey)
                .Distinct());

        return blockedIds.ToHashSet();
    }

    private async Task<Guid?> GetOnsiteAssessmentTaskCategoryIdAsync()
    {
        var category = await ResTaskCategoryRepository.FirstOrDefaultAsync(c => c.Code == "CLAIM_ONSITE_ASSESSMENT_TASK");
        return category?.Id;
    }

    private async Task<IQueryable<ClaimEntity>> ApplyClaimFiltersAsync(IQueryable<ClaimEntity> query, GetOnsiteAssessmentTasksInput input)
    {
        if (input.LobId.HasValue)
        {
            query = query.Where(c => c.LobId == input.LobId.Value);
        }

        if (input.InsurerId.HasValue)
        {
            query = query.Where(c => c.InsurerId == input.InsurerId.Value);
        }

        if (input.ProcessClaimType.HasValue)
        {
            query = query.Where(c => c.ProcessClaimType == input.ProcessClaimType.Value);
        }

        if (!string.IsNullOrWhiteSpace(input.NotifierPhone))
        {
            query = query.Where(c => EF.Functions.ILike(c.NotifierPhone, $"%{input.NotifierPhone}%"));
        }

        if (input.OpenDateFrom.HasValue)
        {
            query = query.Where(c => c.OpenDate >= input.OpenDateFrom.Value.Date);
        }

        if (input.OpenDateTo.HasValue)
        {
            query = query.Where(c => c.OpenDate <= input.OpenDateTo.Value.Date.AddDays(1).AddTicks(-1));
        }

        if (!string.IsNullOrWhiteSpace(input.Code))
        {
            query = query.Where(c => EF.Functions.ILike(c.Code, $"%{input.Code}%"));
        }

        if (input.ProcessDeptId.HasValue)
        {
            query = query.Where(c => c.ProcessDeptId == input.ProcessDeptId.Value);
        }

        if (input.OpenEmployeeId.HasValue)
        {
            query = query.Where(c => c.OpenEmployeeId == input.OpenEmployeeId.Value);
        }

        if (!string.IsNullOrWhiteSpace(input.CarPlate) || !string.IsNullOrWhiteSpace(input.Vin) || !string.IsNullOrWhiteSpace(input.EngineNumber))
        {
            var claimIncidentQuery = await ClaimIncidentRepository.GetQueryableAsync();
            var riskMotorQuery = await ClaimIncidentRiskMotorRepository.GetQueryableAsync();

            var claimRiskMotorData = await AsyncExecuter.ToListAsync(
                from ci in claimIncidentQuery
                join rm in riskMotorQuery on ci.Id equals rm.IncidentObjectId into riskMotors
                from rm in riskMotors.DefaultIfEmpty()
                select new { ClaimId = ci.IncidentId, RiskMotor = rm });

            var matchingClaimIds = new List<Guid>();
            foreach (var item in claimRiskMotorData)
            {
                var match = true;

                if (!string.IsNullOrWhiteSpace(input.CarPlate))
                {
                    var normalizedInput = NormalizeCarInfo(input.CarPlate);
                    var normalizedValue = item.RiskMotor != null && !string.IsNullOrEmpty(item.RiskMotor.CarPlate)
                        ? NormalizeCarInfo(item.RiskMotor.CarPlate)
                        : string.Empty;

                    if (normalizedValue != normalizedInput)
                    {
                        match = false;
                    }
                }

                if (match && !string.IsNullOrWhiteSpace(input.Vin))
                {
                    var normalizedInput = NormalizeCarInfo(input.Vin);
                    var normalizedValue = item.RiskMotor != null && !string.IsNullOrEmpty(item.RiskMotor.Vin)
                        ? NormalizeCarInfo(item.RiskMotor.Vin)
                        : string.Empty;

                    if (normalizedValue != normalizedInput)
                    {
                        match = false;
                    }
                }

                if (match && !string.IsNullOrWhiteSpace(input.EngineNumber))
                {
                    var normalizedInput = NormalizeCarInfo(input.EngineNumber);
                    var normalizedValue = item.RiskMotor != null && !string.IsNullOrEmpty(item.RiskMotor.EngineNumber)
                        ? NormalizeCarInfo(item.RiskMotor.EngineNumber)
                        : string.Empty;

                    if (normalizedValue != normalizedInput)
                    {
                        match = false;
                    }
                }

                if (match)
                {
                    matchingClaimIds.Add(item.ClaimId);
                }
            }

            if (matchingClaimIds.Count == 0)
            {
                query = query.Where(_ => false);
            }
            else
            {
                query = query.Where(c => matchingClaimIds.Contains(c.Id));
            }
        }

        return query;
    }

    private async Task<ClaimAdjustAtLocation?> GetLatestClaimAdjustAtLocationAsync(Guid claimId)
    {
        var query = await ClaimAdjustAtLocationRepository.GetQueryableAsync();
        return await AsyncExecuter.FirstOrDefaultAsync(
            query
                .Where(a => a.ClaimId == claimId)
                .OrderByDescending(a => a.CreationTime)
                .ThenByDescending(a => a.Id));
    }

    private async Task<(Guid ClaimId, Guid AdjustAtLocationId, ClaimAdjustAtLocation? Adjust)> ResolveWorkTaskContextAsync(WorkTask workTask)
    {
        var adjust = await ClaimAdjustAtLocationRepository.FirstOrDefaultAsync(x => x.Id == workTask.BusinessKey);
        if (adjust != null)
        {
            return (adjust.ClaimId, adjust.Id, adjust);
        }

        adjust = await GetLatestClaimAdjustAtLocationAsync(workTask.BusinessKey);
        return (workTask.BusinessKey, adjust?.Id ?? Guid.Empty, adjust);
    }

    private static IEnumerable<OnsiteAssessmentTaskContext> ApplyTaskSorting(
        IEnumerable<OnsiteAssessmentTaskContext> tasks,
        IReadOnlyDictionary<Guid, ClaimEntity> claimById,
        string sorting)
    {
        var parts = (sorting ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var field = parts.Length > 0 ? parts[0].Trim().ToLowerInvariant() : "opendate";
        var order = parts.Length > 1 ? parts[1].Trim().ToLowerInvariant() : "desc";
        var asc = order == "asc";

        return field switch
        {
            "code" => asc
                ? tasks.OrderBy(x => claimById.GetValueOrDefault(x.ClaimId)?.Code).ThenByDescending(x => x.CreationTime)
                : tasks.OrderByDescending(x => claimById.GetValueOrDefault(x.ClaimId)?.Code).ThenByDescending(x => x.CreationTime),
            "notifydate" => asc
                ? tasks.OrderBy(x => claimById.GetValueOrDefault(x.ClaimId)?.NotifyDate).ThenByDescending(x => x.CreationTime)
                : tasks.OrderByDescending(x => claimById.GetValueOrDefault(x.ClaimId)?.NotifyDate).ThenByDescending(x => x.CreationTime),
            "creationtime" => asc
                ? tasks.OrderBy(x => x.CreationTime)
                : tasks.OrderByDescending(x => x.CreationTime),
            _ => asc
                ? tasks.OrderBy(x => claimById.GetValueOrDefault(x.ClaimId)?.OpenDate).ThenByDescending(x => x.CreationTime)
                : tasks.OrderByDescending(x => claimById.GetValueOrDefault(x.ClaimId)?.OpenDate).ThenByDescending(x => x.CreationTime),
        };
    }

    private static string NormalizeOptionalText(string? value, bool fallbackNa = true)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return fallbackNa ? "N/A" : string.Empty;
        }

        return trimmed;
    }

    private static bool IsOnsiteProfileDocumentType(ResDocumentType? docType)
    {
        if (docType == null)
        {
            return false;
        }

        return string.Equals(docType.DocumentGroupCode, OnsiteProfileDocumentGroupCode, StringComparison.OrdinalIgnoreCase)
            || string.Equals(docType.Code, OnsiteProfileDocumentGroupCode, StringComparison.OrdinalIgnoreCase);
    }

    private async Task UpsertOnsiteDocumentsAsync(Guid claimId, Guid adjustAtLocationId, SaveOnsiteAssessmentInput input)
    {
        var imageDocType = await ResDocumentTypeRepository.FirstOrDefaultAsync(x =>
            x.Code == "CAR_ASSESSMENT_IMAGE");
        var inputImageDocumentTypeIds = (input.Images ?? new List<SaveOnsiteImageInput>())
            .Where(x => x.DocumentTypeId.HasValue)
            .Select(x => x.DocumentTypeId!.Value)
            .Distinct()
            .ToList();
        var inputImageDocumentTypes = inputImageDocumentTypeIds.Count == 0
            ? new Dictionary<Guid, ResDocumentType>()
            : (await AsyncExecuter.ToListAsync(
                (await ResDocumentTypeRepository.GetQueryableAsync())
                .Where(x =>
                    inputImageDocumentTypeIds.Contains(x.Id) &&
                    (x.DocumentGroupCode == "CAR_ASSESSMENT_IMAGE" || x.Code == "CAR_ASSESSMENT_IMAGE"))))
            .ToDictionary(x => x.Id, x => x);

        var claimDocQuery = await ClaimDocumentRepository.GetQueryableAsync();
        var existingClaimDocs = await AsyncExecuter.ToListAsync(
            claimDocQuery.Where(x =>
                x.ClaimId == claimId &&
                x.AdjustAtLocationId == adjustAtLocationId));
        var existingById = existingClaimDocs.ToDictionary(x => x.Id, x => x);
        var existingByDocId = existingClaimDocs
            .Where(x => x.DocumentId.HasValue)
            .GroupBy(x => x.DocumentId!.Value)
            .ToDictionary(x => x.Key, x => x.First());
        var existingMetadataByDocTypeId = existingClaimDocs
            .Where(x => !x.DocumentId.HasValue && x.DocumentTypeId.HasValue)
            .GroupBy(x => x.DocumentTypeId!.Value)
            .ToDictionary(x => x.Key, x => x.First());

        var profileDocumentTypeIds = (input.Documents ?? new List<SaveOnsiteDocumentInput>())
            .Where(x => x.DocumentTypeId.HasValue)
            .Select(x => x.DocumentTypeId!.Value)
            .Distinct()
            .ToList();
        var profileDocumentTypes = profileDocumentTypeIds.Count == 0
            ? new Dictionary<Guid, ResDocumentType>()
            : (await AsyncExecuter.ToListAsync(
                (await ResDocumentTypeRepository.GetQueryableAsync())
                .Where(x => profileDocumentTypeIds.Contains(x.Id) && x.DocumentGroupCode == OnsiteProfileDocumentGroupCode)))
            .ToDictionary(x => x.Id, x => x);
        var profileDocumentIds = (input.Documents ?? new List<SaveOnsiteDocumentInput>())
            .Where(x => x.DocumentId.HasValue)
            .Select(x => x.DocumentId!.Value)
            .Distinct()
            .ToList();
        var profileResDocs = profileDocumentIds.Count == 0
            ? new Dictionary<Guid, ResDocument>()
            : (await AsyncExecuter.ToListAsync(
                (await ResDocumentRepository.GetQueryableAsync()).Where(x => profileDocumentIds.Contains(x.Id))))
            .ToDictionary(x => x.Id, x => x);

        // 1) Upsert profile docs (metadata + link to uploaded res_document)
        foreach (var item in input.Documents ?? new List<SaveOnsiteDocumentInput>())
        {
            var normalizedComplete = NormalizeYn(item.Complete);
            var normalizedIsCopy = NormalizeYn(item.IsCopy);
            var note = string.IsNullOrWhiteSpace(item.Note) ? null : item.Note.Trim();
            if (!item.ClaimDocumentId.HasValue && !item.DocumentId.HasValue && !item.DocumentTypeId.HasValue)
            {
                continue;
            }

            if (item.DocumentId.HasValue &&
                item.DocumentTypeId.HasValue &&
                profileResDocs.TryGetValue(item.DocumentId.Value, out var profileResDoc) &&
                profileDocumentTypes.TryGetValue(item.DocumentTypeId.Value, out var profileDocType))
            {
                if (profileResDoc.DocumentTypeId != profileDocType.Id)
                {
                    _logger.LogInformation(
                        "[OnsiteAssessment][Documents][Upsert][v2-dynamic-document-types] Reassign profile attachment document type. DocumentId={DocumentId}, FromTypeId={FromTypeId}, ToTypeId={ToTypeId}, Code={Code}",
                        profileResDoc.Id,
                        profileResDoc.DocumentTypeId,
                        profileDocType.Id,
                        profileDocType.Code);
                    profileResDoc.UpdateDocumentTypeId(profileDocType.Id);
                }

                if (!string.Equals(profileResDoc.GroupCode, profileDocType.Code, StringComparison.OrdinalIgnoreCase))
                {
                    profileResDoc.UpdateGroupCode(profileDocType.Code);
                }

                await ResDocumentRepository.UpdateAsync(profileResDoc);
            }

            if (item.ClaimDocumentId.HasValue &&
                existingById.TryGetValue(item.ClaimDocumentId.Value, out var existing))
            {
                existing.UpdateDocumentTypeId(item.DocumentTypeId);
                existing.UpdateMetadata(note, normalizedComplete, normalizedIsCopy, item.IssueDate);
                await ClaimDocumentRepository.UpdateAsync(existing);
                continue;
            }

            if (item.DocumentId.HasValue &&
                existingByDocId.TryGetValue(item.DocumentId.Value, out var existingByDocument))
            {
                existingByDocument.UpdateDocumentTypeId(item.DocumentTypeId);
                existingByDocument.UpdateMetadata(note, normalizedComplete, normalizedIsCopy, item.IssueDate);
                await ClaimDocumentRepository.UpdateAsync(existingByDocument);
                continue;
            }

            if (!item.DocumentId.HasValue &&
                item.DocumentTypeId.HasValue &&
                existingMetadataByDocTypeId.TryGetValue(item.DocumentTypeId.Value, out var existingMetadata))
            {
                existingMetadata.UpdateDocumentTypeId(item.DocumentTypeId);
                existingMetadata.UpdateMetadata(note, normalizedComplete, normalizedIsCopy, item.IssueDate);
                await ClaimDocumentRepository.UpdateAsync(existingMetadata);
                continue;
            }

            var newProfileClaimDoc = new ClaimDocument(
                GuidGenerator.Create(),
                claimId: claimId,
                documentId: item.DocumentId,
                documentTypeId: item.DocumentTypeId,
                adjustAtLocationId: adjustAtLocationId,
                note: note,
                complete: normalizedComplete,
                isCopy: normalizedIsCopy,
                issueDate: item.IssueDate
            );
            await ClaimDocumentRepository.InsertAsync(newProfileClaimDoc);
            if (item.DocumentId.HasValue)
            {
                existingByDocId[item.DocumentId.Value] = newProfileClaimDoc;
            }
            else if (item.DocumentTypeId.HasValue)
            {
                existingMetadataByDocTypeId[item.DocumentTypeId.Value] = newProfileClaimDoc;
            }
            existingById[newProfileClaimDoc.Id] = newProfileClaimDoc;
        }

        // 2) Upsert onsite image links
        foreach (var item in input.Images ?? new List<SaveOnsiteImageInput>())
        {
            if (!item.DocumentId.HasValue)
            {
                continue;
            }

            if (item.ClaimDocumentId.HasValue && existingById.ContainsKey(item.ClaimDocumentId.Value))
            {
                var existingImageClaimDoc = existingById[item.ClaimDocumentId.Value];
                if (item.DocumentTypeId.HasValue)
                {
                    existingImageClaimDoc.UpdateDocumentTypeId(item.DocumentTypeId);
                    await ClaimDocumentRepository.UpdateAsync(existingImageClaimDoc);
                }
                continue;
            }

            if (existingByDocId.ContainsKey(item.DocumentId.Value))
            {
                var existingImageClaimDoc = existingByDocId[item.DocumentId.Value];
                if (item.DocumentTypeId.HasValue)
                {
                    existingImageClaimDoc.UpdateDocumentTypeId(item.DocumentTypeId);
                    await ClaimDocumentRepository.UpdateAsync(existingImageClaimDoc);
                }
                continue;
            }

            var newImageClaimDoc = new ClaimDocument(
                GuidGenerator.Create(),
                claimId: claimId,
                documentId: item.DocumentId.Value,
                documentTypeId: item.DocumentTypeId,
                adjustAtLocationId: adjustAtLocationId,
                note: null,
                complete: null,
                isCopy: null,
                issueDate: null
            );
            await ClaimDocumentRepository.InsertAsync(newImageClaimDoc);
            existingByDocId[item.DocumentId.Value] = newImageClaimDoc;
            existingById[newImageClaimDoc.Id] = newImageClaimDoc;
        }

        // 2.1) Delete removed onsite images (present in DB but no longer present in input.Images)
        var inputImageClaimDocIds = (input.Images ?? new List<SaveOnsiteImageInput>())
            .Where(x => x.ClaimDocumentId.HasValue)
            .Select(x => x.ClaimDocumentId!.Value)
            .ToHashSet();
        var inputImageDocIds = (input.Images ?? new List<SaveOnsiteImageInput>())
            .Where(x => x.DocumentId.HasValue)
            .Select(x => x.DocumentId!.Value)
            .ToHashSet();

        var existingImageDocs = await AsyncExecuter.ToListAsync(
            (await ResDocumentRepository.GetQueryableAsync())
            .Where(x =>
                x.GroupCode == "CAR_ASSESSMENT_IMAGE" ||
                (imageDocType != null && x.DocumentTypeId == imageDocType.Id) ||
                inputImageDocumentTypeIds.Contains(x.DocumentTypeId)));
        var existingImageDocIds = existingImageDocs.Select(x => x.Id).ToHashSet();

        var claimDocsToDelete = existingClaimDocs
            .Where(cd =>
                cd.DocumentId.HasValue &&
                existingImageDocIds.Contains(cd.DocumentId.Value) &&
                !inputImageClaimDocIds.Contains(cd.Id) &&
                !inputImageDocIds.Contains(cd.DocumentId.Value))
            .ToList();

        foreach (var item in claimDocsToDelete)
        {
            await ClaimDocumentRepository.DeleteAsync(item);
            if (item.DocumentId.HasValue)
            {
                var resDoc = existingImageDocs.FirstOrDefault(x => x.Id == item.DocumentId.Value);
                if (resDoc != null)
                {
                    await ResDocumentRepository.DeleteAsync(resDoc);
                }
            }
        }

        // 3) Align uploaded res_document type/group for classification in detail API
        var uploadedDocIds = (input.Documents ?? new List<SaveOnsiteDocumentInput>())
            .Where(x => x.DocumentId.HasValue)
            .Select(x => x.DocumentId!.Value)
            .Concat((input.Images ?? new List<SaveOnsiteImageInput>())
                .Where(x => x.DocumentId.HasValue)
                .Select(x => x.DocumentId!.Value))
            .Distinct()
            .ToList();

        if (uploadedDocIds.Count == 0)
        {
            return;
        }

        var resDocs = await AsyncExecuter.ToListAsync(
            (await ResDocumentRepository.GetQueryableAsync()).Where(x => uploadedDocIds.Contains(x.Id)));

        foreach (var resDoc in resDocs)
        {
            var imageInput = (input.Images ?? new List<SaveOnsiteImageInput>())
                .FirstOrDefault(x => x.DocumentId == resDoc.Id);
            var isImageInput = imageInput != null;

            if (isImageInput)
            {
                var targetImageDocType = imageInput!.DocumentTypeId.HasValue &&
                    inputImageDocumentTypes.TryGetValue(imageInput.DocumentTypeId.Value, out var matchedImageDocType)
                        ? matchedImageDocType
                        : imageDocType;

                if (targetImageDocType != null)
                {
                    resDoc.UpdateDocumentTypeId(targetImageDocType.Id);
                }
                resDoc.UpdateGroupCode("CAR_ASSESSMENT_IMAGE");
                await ResDocumentRepository.UpdateAsync(resDoc);
                continue;
            }

            // Profile document type/group is aligned above from the row DocumentTypeId sent by FE.
        }
    }

    private static string? NormalizeYn(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return string.Equals(value.Trim(), "Y", StringComparison.OrdinalIgnoreCase) ? "Y" : "N";
    }

    private static void ValidateSaveInput(SaveOnsiteAssessmentInput input)
    {
        var missing = new List<string>();

        if (string.IsNullOrWhiteSpace(input.DriverName)) missing.Add(nameof(input.DriverName));
        if (string.IsNullOrWhiteSpace(input.DriverSex)) missing.Add(nameof(input.DriverSex));
        if (string.IsNullOrWhiteSpace(input.DriverPhone)) missing.Add(nameof(input.DriverPhone));
        if (string.IsNullOrWhiteSpace(input.DriverIdNo)) missing.Add(nameof(input.DriverIdNo));
        if (string.IsNullOrWhiteSpace(input.DriverLicenseNo)) missing.Add(nameof(input.DriverLicenseNo));
        if (!input.DriverLicenseEffectDate.HasValue) missing.Add(nameof(input.DriverLicenseEffectDate));
        if (!input.DriverLicenseExpireDate.HasValue) missing.Add(nameof(input.DriverLicenseExpireDate));
        if (string.IsNullOrWhiteSpace(input.DriverLicenseLevel)) missing.Add(nameof(input.DriverLicenseLevel));
        if (string.IsNullOrWhiteSpace(input.CarRegistryNo)) missing.Add(nameof(input.CarRegistryNo));
        if (!input.CarRegistryEffectDate.HasValue) missing.Add(nameof(input.CarRegistryEffectDate));
        if (!input.CarRegistryExpireDate.HasValue) missing.Add(nameof(input.CarRegistryExpireDate));
        if (input.LossPositions == null || input.LossPositions.Count == 0) missing.Add(nameof(input.LossPositions));
        if (string.IsNullOrWhiteSpace(input.WitnessTestimony)) missing.Add(nameof(input.WitnessTestimony));
        if (string.IsNullOrWhiteSpace(input.CauseDescription)) missing.Add(nameof(input.CauseDescription));
        if (string.IsNullOrWhiteSpace(input.Description)) missing.Add(nameof(input.Description));
        if (string.IsNullOrWhiteSpace(input.LocationDescription)) missing.Add(nameof(input.LocationDescription));
        if (string.IsNullOrWhiteSpace(input.DamageDescription)) missing.Add(nameof(input.DamageDescription));
        if (string.IsNullOrWhiteSpace(input.PartiesInvolvedDescription)) missing.Add(nameof(input.PartiesInvolvedDescription));
        if (string.IsNullOrWhiteSpace(input.AddressPlan)) missing.Add(nameof(input.AddressPlan));
        if (string.IsNullOrWhiteSpace(input.CustomerRecommendation)) missing.Add(nameof(input.CustomerRecommendation));
        if (missing.Count > 0)
        {
            throw new Volo.Abp.UserFriendlyException($"Thiếu dữ liệu bắt buộc: {string.Join(", ", missing)}");
        }
    }

    private static IQueryable<ClaimEntity> ApplyClaimSorting(IQueryable<ClaimEntity> query, string sorting)
    {
        var parts = sorting.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var field = parts.Length > 0 ? parts[0].ToLowerInvariant() : "opendate";
        var order = parts.Length > 1 && parts[1].ToLowerInvariant() == "asc" ? "asc" : "desc";

        return field switch
        {
            "code" => order == "asc" ? query.OrderBy(c => c.Code) : query.OrderByDescending(c => c.Code),
            "opendate" => order == "asc" ? query.OrderBy(c => c.OpenDate) : query.OrderByDescending(c => c.OpenDate),
            "notifydate" => order == "asc" ? query.OrderBy(c => c.NotifyDate) : query.OrderByDescending(c => c.NotifyDate),
            _ => query.OrderByDescending(c => c.OpenDate)
        };
    }

    private static string NormalizeCarInfo(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return Regex.Replace(value, @"[^A-Za-z0-9]", "").ToUpperInvariant();
    }

    private sealed class OnsiteAssessmentTaskContext
    {
        public Guid Id { get; set; }
        public Guid ClaimId { get; set; }
        public Guid ClaimAdjustAtLocationId { get; set; }
        public WorkTaskStatus Status { get; set; }
        public Guid ReporterId { get; set; }
        public Guid? AssigneeId { get; set; }
        public DateTime CreationTime { get; set; }
    }

    private sealed class ClaimAdjustLink
    {
        public Guid Id { get; set; }
        public Guid ClaimId { get; set; }
    }

}
