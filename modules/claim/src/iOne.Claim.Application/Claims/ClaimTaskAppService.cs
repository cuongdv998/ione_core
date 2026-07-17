using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ClosedXML.Excel;
using iOne.Claim.Claims;
using iOne.Claim.Localization;
using iOne.ClaimAdjustAtLocations;
using iOne.ClaimFolders;
using iOne.ClaimIncidentRiskMotors;
using iOne.ClaimIncidents;
using iOne.Claims;
using iOne.HrDepartments;
using iOne.HrEmployees;
using iOne.Policies;
using iOne.ProCoverages;
using iOne.ProLineOfBusinesses;
using iOne.ProProducts;
using iOne.ResObjectTypes;
using iOne.ResPartners;
using iOne.ResTaskCategories;
using iOne.Workflow;
using iOne.WorkInstances;
using iOne.WorkTasks;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Content;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;
using ClaimEntity = iOne.Claims.Claim;

namespace iOne.Claim.Claims;

public class ClaimTaskAppService : ApplicationService, IClaimTaskAppService
{
    protected IRepository<WorkTask, Guid> WorkTaskRepository { get; }
    protected IRepository<ResTaskCategory, Guid> ResTaskCategoryRepository { get; }
    protected IRepository<ClaimEntity, Guid> ClaimRepository { get; }
    protected IRepository<ClaimFolder, Guid> ClaimFolderRepository { get; }
    protected IRepository<ClaimIncident, Guid> ClaimIncidentRepository { get; }
    protected IRepository<ClaimIncidentRiskMotor, Guid> ClaimIncidentRiskMotorRepository { get; }
    protected IRepository<ProProduct, Guid> ProductRepository { get; }
    protected IRepository<HrEmployee, Guid> EmployeeRepository { get; }
    protected IRepository<HrDepartment, Guid> DepartmentRepository { get; }
    protected IRepository<ClaimAdjustAtLocation, Guid> ClaimAdjustAtLocationRepository { get; }
    protected IRepository<PolicyVersion, Guid> PolicyVersionRepository { get; }
    protected IRepository<PolicyProduct, Guid> PolicyProductRepository { get; }
    protected IRepository<PolicyCoverage, Guid> PolicyCoverageRepository { get; }
    protected IRepository<ProCoverage, Guid> ProCoverageRepository { get; }
    protected IRepository<ResObjectType, Guid> ResObjectTypeRepository { get; }
    private readonly IElsaWorkflowService _elsaWorkflowService;
    private readonly IWorkInstanceRepository _workInstanceRepository;
    private readonly IClaimFolderAppService _claimFolderAppService;
    private readonly IOnsiteAssessmentAssignmentService _onsiteAssessmentAssignmentService;

    public ClaimTaskAppService(
        IRepository<WorkTask, Guid> workTaskRepository,
        IRepository<ResTaskCategory, Guid> resTaskCategoryRepository,
        IRepository<ClaimEntity, Guid> claimRepository,
        IRepository<ClaimFolder, Guid> claimFolderRepository,
        IRepository<ClaimIncident, Guid> claimIncidentRepository,
        IRepository<ClaimIncidentRiskMotor, Guid> claimIncidentRiskMotorRepository,
        IRepository<ProProduct, Guid> productRepository,
        IRepository<HrEmployee, Guid> employeeRepository,
        IRepository<HrDepartment, Guid> departmentRepository,
        IRepository<ClaimAdjustAtLocation, Guid> claimAdjustAtLocationRepository,
        IRepository<PolicyVersion, Guid> policyVersionRepository,
        IRepository<PolicyProduct, Guid> policyProductRepository,
        IRepository<PolicyCoverage, Guid> policyCoverageRepository,
        IRepository<ProCoverage, Guid> proCoverageRepository,
        IRepository<ResObjectType, Guid> resObjectTypeRepository,
        IElsaWorkflowService elsaWorkflowService,
        IWorkInstanceRepository workInstanceRepository,
        IClaimFolderAppService claimFolderAppService,
        IOnsiteAssessmentAssignmentService onsiteAssessmentAssignmentService)
    {
        WorkTaskRepository = workTaskRepository;
        ResTaskCategoryRepository = resTaskCategoryRepository;
        ClaimRepository = claimRepository;
        ClaimFolderRepository = claimFolderRepository;
        ClaimIncidentRepository = claimIncidentRepository;
        ClaimIncidentRiskMotorRepository = claimIncidentRiskMotorRepository;
        ProductRepository = productRepository;
        EmployeeRepository = employeeRepository;
        DepartmentRepository = departmentRepository;
        ClaimAdjustAtLocationRepository = claimAdjustAtLocationRepository;
        PolicyVersionRepository = policyVersionRepository;
        PolicyProductRepository = policyProductRepository;
        PolicyCoverageRepository = policyCoverageRepository;
        ProCoverageRepository = proCoverageRepository;
        ResObjectTypeRepository = resObjectTypeRepository;
        LocalizationResource = typeof(ClaimResource);
        _elsaWorkflowService = elsaWorkflowService;
        _workInstanceRepository = workInstanceRepository;
        _claimFolderAppService = claimFolderAppService;
        _onsiteAssessmentAssignmentService = onsiteAssessmentAssignmentService;
    }

    public virtual async Task<ClaimTaskDto> GetAsync(Guid workTaskId)
    {
        var currentEmployeeId = await GetCurrentEmployeeIdAsync();
        var workTask = await WorkTaskRepository.GetAsync(workTaskId);
        if (workTask.BusinessCode != "CLAIM_ASSIGN" || workTask.AssigneeId != currentEmployeeId)
            throw new Volo.Abp.UserFriendlyException(L["ClaimTask:NotAuthorized"].Value);

        var claimQuery = await ClaimRepository.GetQueryableAsync();
        var claim = await AsyncExecuter.FirstOrDefaultAsync(
            claimQuery
                .Include(c => c.Lob)
                .Include(c => c.Insurer)
                .Include(c => c.OpenEmployee)
                .Where(c => c.Id == workTask.BusinessKey));
        if (claim == null)
            throw new Volo.Abp.UserFriendlyException(L["ClaimTask:ClaimNotFound"].Value);

        var incident = await ClaimIncidentRepository.FirstOrDefaultAsync(ci => ci.IncidentId == claim.Id);
        ClaimIncidentRiskMotor? riskMotor = null;
        if (incident != null)
            riskMotor = await ClaimIncidentRiskMotorRepository.FirstOrDefaultAsync(rm => rm.IncidentObjectId == incident.Id);
        var claimFolder = await ClaimFolderRepository.FirstOrDefaultAsync(cf => cf.ClaimId == claim.Id);
        ProProduct? product = null;
        if (claimFolder?.ProductId != null)
            product = await ProductRepository.FirstOrDefaultAsync(p => p.Id == claimFolder.ProductId.Value);

        return new ClaimTaskDto
        {
            Id = workTask.Id,
            ClaimId = claim.Id,
            Code = claim.Code,
            FolderNo = claimFolder?.FolderNo,
            InsurerName = claim.Insurer?.Name,
            InsurerCode = claim.Insurer?.Code,
            LobName = claim.Lob?.Name,
            ProductName = product?.Name,
            NotifierName = claim.NotifierName,
            OpenDate = claim.OpenDate,
            NotifyDate = claim.NotifyDate,
            CarPlate = riskMotor?.CarPlate,
            IncidentDate = incident?.IncidentDate,
            OnLocation = incident?.OnLocation,
            OpenEmployeeName = claim.OpenEmployee?.FullName,
            OpenEmployeePhone = claim.OpenEmployee?.Phone,
            ProcessClaimType = claim.ProcessClaimType,
            ClaimStatus = claim.Status,
            WorkTaskStatus = workTask.Status,
            HasActiveOnsiteAssessmentTask = await HasActiveOnsiteAssessmentByClaimIdAsync(workTask.BusinessKey),
            HasFinishedOnsiteAssessment = await HasFinishedOnsiteAssessmentByClaimIdAsync(workTask.BusinessKey)
        };
    }

    public virtual async Task<PagedResultDto<ClaimTaskDto>> GetListAsync(GetClaimTasksInput input)
    {
        var currentEmployeeId = await GetCurrentEmployeeIdAsync();
        var categoryId = await GetClaimAssignTaskCategoryIdAsync();
        if (categoryId == null)
        {
            return new PagedResultDto<ClaimTaskDto>(0, new List<ClaimTaskDto>());
        }

        var workTaskQuery = await WorkTaskRepository.GetQueryableAsync();
        workTaskQuery = workTaskQuery
            .Where(wt => wt.BusinessCode == "CLAIM_ASSIGN"
                && wt.AssigneeId == currentEmployeeId
                && wt.TaskCategoryId == categoryId.Value
                && (!input.WorkTaskStatus.HasValue || wt.Status == input.WorkTaskStatus.Value)
                && (wt.Status == WorkTaskStatus.New
                    || wt.Status == WorkTaskStatus.InProgress
                    || wt.Status == WorkTaskStatus.Completed
                    || wt.Status == WorkTaskStatus.Accepted
                    || wt.Status == WorkTaskStatus.WaitApprove
                    || wt.Status == WorkTaskStatus.Approved
                    || wt.Status == WorkTaskStatus.Pending));
        var assignedPairs = await AsyncExecuter.ToListAsync(
            workTaskQuery.Select(wt => new { wt.Id, ClaimId = wt.BusinessKey, wt.Status }));
        var assignedClaimIds = assignedPairs.Select(x => x.ClaimId).Distinct().ToList();
        var claimIdToWorkTask = assignedPairs.GroupBy(x => x.ClaimId).ToDictionary(g => g.Key, g => g.First());

        if (assignedClaimIds.Count == 0)
        {
            return new PagedResultDto<ClaimTaskDto>(0, new List<ClaimTaskDto>());
        }

        var claimQuery = await ClaimRepository.GetQueryableAsync();
        claimQuery = claimQuery
            .Include(c => c.Lob)
            .Include(c => c.Insurer)
            .Include(c => c.OpenEmployee)
            .Where(c => assignedClaimIds.Contains(c.Id));

        claimQuery = await ApplyClaimFiltersAsync(claimQuery, input);

        var totalCount = await AsyncExecuter.CountAsync(claimQuery);
        var sortOrder = string.IsNullOrWhiteSpace(input.Sorting)
            ? "openDate desc"
            : input.Sorting;
        claimQuery = ApplyClaimSorting(claimQuery, sortOrder);
        claimQuery = claimQuery.Skip(input.SkipCount).Take(input.MaxResultCount > 0 ? input.MaxResultCount : 10);
        var claims = await AsyncExecuter.ToListAsync(claimQuery);

        var claimIds = claims.Select(c => c.Id).ToList();
        var claimFolders = await AsyncExecuter.ToListAsync(
            (await ClaimFolderRepository.GetQueryableAsync()).Where(cf => claimIds.Contains(cf.ClaimId)));
        var productIds = claimFolders.Where(cf => cf.ProductId.HasValue).Select(cf => cf.ProductId!.Value).Distinct().ToList();
        var products = productIds.Count == 0
            ? new List<ProProduct>()
            : await AsyncExecuter.ToListAsync((await ProductRepository.GetQueryableAsync()).Where(p => productIds.Contains(p.Id)));
        var incidents = await AsyncExecuter.ToListAsync(
            (await ClaimIncidentRepository.GetQueryableAsync()).Where(ci => claimIds.Contains(ci.IncidentId)));
        var incidentIds = incidents.Select(i => i.Id).ToList();
        var riskMotors = await AsyncExecuter.ToListAsync(
            (await ClaimIncidentRiskMotorRepository.GetQueryableAsync())
                .Where(rm => incidentIds.Contains(rm.IncidentObjectId ?? Guid.Empty)));
        var incidentByClaimId = incidents.ToDictionary(i => i.IncidentId);
        var riskMotorByIncidentId = riskMotors.GroupBy(r => r.IncidentObjectId ?? Guid.Empty).ToDictionary(g => g.Key, g => g.First());
        var claimFolderByClaimId = claimFolders
            .GroupBy(cf => cf.ClaimId)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.OpenDate).First());
        var productById = products.ToDictionary(p => p.Id, p => p);
        var activeOnsiteAssessmentClaimIds = await GetActiveOnsiteAssessmentClaimIdsAsync(claimIds);
        var finishedOnsiteAssessmentClaimIds = await GetFinishedOnsiteAssessmentClaimIdsAsync(claimIds);

        var dtos = new List<ClaimTaskDto>();
        foreach (var claim in claims)
        {
            if (!claimIdToWorkTask.TryGetValue(claim.Id, out var wtPair))
                continue;
            incidentByClaimId.TryGetValue(claim.Id, out var incident);
            ClaimIncidentRiskMotor? riskMotor = null;
            if (incident != null)
                riskMotorByIncidentId.TryGetValue(incident.Id, out riskMotor);
            claimFolderByClaimId.TryGetValue(claim.Id, out var claimFolder);
            ProProduct? product = null;
            if (claimFolder?.ProductId != null)
                productById.TryGetValue(claimFolder.ProductId.Value, out product);

            dtos.Add(new ClaimTaskDto
            {
                Id = wtPair.Id,
                ClaimId = claim.Id,
                Code = claim.Code,
                FolderNo = claimFolder?.FolderNo,
                InsurerName = claim.Insurer?.Name,
                InsurerCode = claim.Insurer?.Code,
                LobName = claim.Lob?.Name,
                ProductName = product?.Name,
                NotifierName = claim.NotifierName,
                OpenDate = claim.OpenDate,
                NotifyDate = claim.NotifyDate,
                CarPlate = riskMotor?.CarPlate,
                IncidentDate = incident?.IncidentDate,
                OnLocation = incident?.OnLocation,
                OpenEmployeeName = claim.OpenEmployee?.FullName,
                OpenEmployeePhone = claim.OpenEmployee?.Phone,
                ProcessClaimType = claim.ProcessClaimType,
                ClaimStatus = claim.Status,
                WorkTaskStatus = wtPair.Status,
                HasActiveOnsiteAssessmentTask = activeOnsiteAssessmentClaimIds.Contains(claim.Id),
                HasFinishedOnsiteAssessment = finishedOnsiteAssessmentClaimIds.Contains(claim.Id),
                CanCancel = claim.ProcessEmpId == currentEmployeeId
                    && (claim.Status == ClaimStatus.Draft
                        || claim.Status == ClaimStatus.PendingReceive)
            });
        }

        return new PagedResultDto<ClaimTaskDto>(totalCount, dtos);
    }

    public virtual async Task<IRemoteStreamContent> ExportAsync(GetClaimTasksInput input)
    {
        const int pageSize = 1000;
        var allItems = new List<ClaimTaskDto>();
        input.MaxResultCount = pageSize;
        input.SkipCount = 0;
        PagedResultDto<ClaimTaskDto> list;
        do
        {
            list = await GetListAsync(input);
            allItems.AddRange(list.Items);
            input.SkipCount += pageSize;
        }
        while (list.Items.Count == pageSize);

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("ClaimTasks");
        var headers = new[]
        {
            "STT",
            "Mã yêu cầu",
            "Bảo hiểm gốc",
            "Nghiệp vụ BH",
            "Người thông báo",
            "Ngày thông báo",
            "Biển số xe",
            "Ngày tổn thất",
            "Có hiện trường",
            "Người tiếp nhận",
            "Bên xử lý",
            "Trạng thái"
        };
        for (var i = 0; i < headers.Length; i++)
        {
            ws.Cell(1, i + 1).Value = headers[i];
            ws.Cell(1, i + 1).Style.Font.Bold = true;
        }
        var row = 2;
        var index = 1;
        foreach (var t in allItems)
        {
            ws.Cell(row, 1).Value = index;
            ws.Cell(row, 2).Value = t.Code ?? string.Empty;
            ws.Cell(row, 3).Value = t.InsurerCode ?? string.Empty;
            ws.Cell(row, 4).Value = t.LobName ?? string.Empty;
            ws.Cell(row, 5).Value = t.NotifierName ?? string.Empty;
            ws.Cell(row, 6).Value = t.NotifyDate?.ToString("dd/MM/yyyy HH:mm") ?? string.Empty;
            ws.Cell(row, 7).Value = t.CarPlate ?? string.Empty;
            ws.Cell(row, 8).Value = t.IncidentDate?.ToString("dd/MM/yyyy HH:mm:ss") ?? string.Empty;
            ws.Cell(row, 9).Value = t.OnLocation == "Y" ? "Có" : "Không";
            ws.Cell(row, 10).Value = t.OpenEmployeeName ?? string.Empty;
            ws.Cell(row, 11).Value = t.ProcessClaimType == ProcessClaimType.Own ? "Tự xử lý" : "Bảo hiểm gốc xử lý";
            ws.Cell(row, 12).Value = GetClaimStatusName(t.ClaimStatus);
            row++;
            index++;
        }
        ws.Columns().AdjustToContents();

        var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;
        var fileName = $"claim-tasks_{Clock.Now:yyyyMMdd}.xlsx";
        return new RemoteStreamContent(stream, fileName, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
    }

    private static string GetClaimStatusName(ClaimStatus status)
    {
        return status switch
        {
            ClaimStatus.Draft => "Nháp",
            ClaimStatus.PendingReceive => "Chờ tiếp nhận",
            ClaimStatus.InProgress => "Đang xử lý",
            ClaimStatus.Closed => "Đã đóng",
            ClaimStatus.Called => "Đã gọi",
            _ => status.ToString()
        };
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

    private async Task<Guid?> GetClaimAssignTaskCategoryIdAsync()
    {
        var category = await ResTaskCategoryRepository.FirstOrDefaultAsync(c => c.Code == "CLAIM_ASSIGN_TASK");
        return category?.Id;
    }

    public virtual async Task<bool> HasActiveOnsiteAssessmentAsync(Guid workTaskId)
    {
        var currentEmployeeId = await GetCurrentEmployeeIdAsync();
        var workTask = await WorkTaskRepository.GetAsync(workTaskId);
        if (workTask.BusinessCode != "CLAIM_ASSIGN" || workTask.AssigneeId != currentEmployeeId)
        {
            throw new Volo.Abp.UserFriendlyException(L["ClaimTask:NotAuthorized"].Value);
        }

        if (workTask.Status != WorkTaskStatus.InProgress)
        {
            return false;
        }

        return await HasActiveOnsiteAssessmentByClaimIdAsync(workTask.BusinessKey);
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

    private async Task<bool> HasActiveOnsiteAssessmentWorkTaskForAdjustIdAsync(Guid adjustId)
    {
        var onsiteAssessmentQuery = await WorkTaskRepository.GetQueryableAsync();
        return await AsyncExecuter.AnyAsync(
            onsiteAssessmentQuery.Where(wt =>
                wt.BusinessCode == "CLAIM_ONSITE_ASSESSMENT"
                && wt.BusinessKey == adjustId
                && wt.Status != WorkTaskStatus.Rejected
                && wt.Status != WorkTaskStatus.Cancelled
                && wt.Status != WorkTaskStatus.Completed
                && wt.Status != WorkTaskStatus.Return));
    }

    private async Task<bool> HasActiveOnsiteAssessmentByClaimIdAsync(Guid claimId)
    {
        var latestAdjust = await GetLatestClaimAdjustAtLocationAsync(claimId);
        if (latestAdjust == null)
        {
            return false;
        }

        return await HasActiveOnsiteAssessmentWorkTaskForAdjustIdAsync(latestAdjust.Id);
    }

    private async Task<HashSet<Guid>> GetActiveOnsiteAssessmentClaimIdsAsync(List<Guid> claimIds)
    {
        if (claimIds.Count == 0)
        {
            return new HashSet<Guid>();
        }

        var adjustQuery = await ClaimAdjustAtLocationRepository.GetQueryableAsync();
        var adjusts = await AsyncExecuter.ToListAsync(
            adjustQuery.Where(a => claimIds.Contains(a.ClaimId)));

        var latestAdjustIdByClaim = adjusts
            .GroupBy(a => a.ClaimId)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(x => x.CreationTime).ThenByDescending(x => x.Id).First().Id);

        if (latestAdjustIdByClaim.Count == 0)
        {
            return new HashSet<Guid>();
        }

        var latestAdjustIds = latestAdjustIdByClaim.Values.Distinct().ToList();
        var onsiteAssessmentQuery = await WorkTaskRepository.GetQueryableAsync();
        var activeAdjustIds = await AsyncExecuter.ToListAsync(
            onsiteAssessmentQuery
                .Where(wt =>
                    wt.BusinessCode == "CLAIM_ONSITE_ASSESSMENT"
                    && latestAdjustIds.Contains(wt.BusinessKey)
                    && wt.Status != WorkTaskStatus.Rejected
                    && wt.Status != WorkTaskStatus.Cancelled
                    && wt.Status != WorkTaskStatus.Completed
                    && wt.Status != WorkTaskStatus.Return)
                .Select(wt => wt.BusinessKey)
                .Distinct());

        var activeAdjustIdSet = activeAdjustIds.ToHashSet();
        return latestAdjustIdByClaim
            .Where(kv => activeAdjustIdSet.Contains(kv.Value))
            .Select(kv => kv.Key)
            .ToHashSet();
    }

    private async Task<bool> HasFinishedOnsiteAssessmentByClaimIdAsync(Guid claimId)
    {
        var latestAdjust = await GetLatestClaimAdjustAtLocationAsync(claimId);
        return latestAdjust?.Status == ClaimAdjustAtLocationStatus.Done;
    }

    private async Task<HashSet<Guid>> GetFinishedOnsiteAssessmentClaimIdsAsync(List<Guid> claimIds)
    {
        if (claimIds.Count == 0)
        {
            return new HashSet<Guid>();
        }

        var adjustQuery = await ClaimAdjustAtLocationRepository.GetQueryableAsync();
        var adjusts = await AsyncExecuter.ToListAsync(
            adjustQuery.Where(a => claimIds.Contains(a.ClaimId)));

        var result = new HashSet<Guid>();
        foreach (var group in adjusts.GroupBy(a => a.ClaimId))
        {
            var latest = group.OrderByDescending(x => x.CreationTime).ThenByDescending(x => x.Id).First();
            if (latest.Status == ClaimAdjustAtLocationStatus.Done)
            {
                result.Add(group.Key);
            }
        }

        return result;
    }

    private async Task<IQueryable<ClaimEntity>> ApplyClaimFiltersAsync(IQueryable<ClaimEntity> query, GetClaimTasksInput input)
    {
        if (input.LobId.HasValue)
            query = query.Where(c => c.LobId == input.LobId.Value);
        if (input.InsurerId.HasValue)
            query = query.Where(c => c.InsurerId == input.InsurerId.Value);
        if (input.ProcessClaimType.HasValue)
            query = query.Where(c => c.ProcessClaimType == input.ProcessClaimType.Value);
        if (!string.IsNullOrWhiteSpace(input.NotifierPhone))
        {
            query = query.Where(c => EF.Functions.ILike(c.NotifierPhone, $"%{input.NotifierPhone.Trim()}%"));
        }
        if (input.OpenDateFrom.HasValue)
            query = query.Where(c => c.OpenDate >= input.OpenDateFrom.Value.Date);
        if (input.OpenDateTo.HasValue)
            query = query.Where(c => c.OpenDate <= input.OpenDateTo.Value.Date.AddDays(1).AddTicks(-1));
        if (!string.IsNullOrWhiteSpace(input.Code))
        {
            query = query.Where(c => EF.Functions.ILike(c.Code, $"%{input.Code.Trim()}%"));
        }
        var filterClaimStatus = input.Status ?? input.ClaimStatus;
        if (filterClaimStatus.HasValue)
        {
            query = query.Where(c => c.Status == filterClaimStatus.Value);
        }

        if (input.ProcessDeptId.HasValue)
        {
            var departmentIds = await GetDepartmentAndChildIdsAsync(input.ProcessDeptId.Value);
            query = query.Where(c => c.OpenEmployee != null && departmentIds.Contains(c.OpenEmployee.DepartmentId));
        }
        if (input.OpenEmployeeId.HasValue)
        {
            var employee = await EmployeeRepository.FirstOrDefaultAsync(x => x.Id == input.OpenEmployeeId.Value);
            if (employee != null)
            {
                var departmentIds = await GetDepartmentAndChildIdsAsync(employee.DepartmentId);
                query = query.Where(c => c.OpenEmployee != null && departmentIds.Contains(c.OpenEmployee.DepartmentId));
            }
            else
            {
                query = query.Where(c => c.OpenEmployeeId == input.OpenEmployeeId.Value);
            }
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
                    var normalizedValue = item.RiskMotor != null && !string.IsNullOrEmpty(item.RiskMotor.CarPlate) ? NormalizeCarInfo(item.RiskMotor.CarPlate) : string.Empty;
                    if (!normalizedValue.Contains(normalizedInput)) match = false;
                }
                if (match && !string.IsNullOrWhiteSpace(input.Vin))
                {
                    var normalizedInput = NormalizeCarInfo(input.Vin);
                    var normalizedValue = item.RiskMotor != null && !string.IsNullOrEmpty(item.RiskMotor.Vin) ? NormalizeCarInfo(item.RiskMotor.Vin) : string.Empty;
                    if (normalizedValue != normalizedInput) match = false;
                }
                if (match && !string.IsNullOrWhiteSpace(input.EngineNumber))
                {
                    var normalizedInput = NormalizeCarInfo(input.EngineNumber);
                    var normalizedValue = item.RiskMotor != null && !string.IsNullOrEmpty(item.RiskMotor.EngineNumber) ? NormalizeCarInfo(item.RiskMotor.EngineNumber) : string.Empty;
                    if (normalizedValue != normalizedInput) match = false;
                }
                if (match)
                    matchingClaimIds.Add(item.ClaimId);
            }
            if (matchingClaimIds.Count == 0)
                query = query.Where(_ => false);
            else
                query = query.Where(c => matchingClaimIds.Contains(c.Id));
        }

        return query;
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
            "claimstatus" => order == "asc" ? query.OrderBy(c => c.Status) : query.OrderByDescending(c => c.Status),
            _ => query.OrderByDescending(c => c.OpenDate)
        };
    }

    private static string NormalizeCarInfo(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;
        return Regex.Replace(value, @"[^A-Za-z0-9]", "").ToUpperInvariant();
    }

    private async Task<List<Guid>> GetDepartmentAndChildIdsAsync(Guid departmentId)
    {
        var list = new List<Guid> { departmentId };
        var all = await DepartmentRepository.GetListAsync();
        foreach (var child in all.Where(x => x.ParentId == departmentId))
        {
            list.Add(child.Id);
            list.AddRange(await GetDepartmentAndChildIdsAsync(child.Id));
        }
        return list.Distinct().ToList();
    }

    public virtual async Task RejectAsync(Guid workTaskId, RejectClaimTaskInput input)
    {
        var currentEmployeeId = await GetCurrentEmployeeIdAsync();
        var workTask = await WorkTaskRepository.GetAsync(workTaskId);
        if (workTask.BusinessCode != "CLAIM_ASSIGN" || workTask.AssigneeId != currentEmployeeId)
            throw new Volo.Abp.UserFriendlyException(L["ClaimTask:NotAuthorized"].Value);
        if (workTask.Status != WorkTaskStatus.New)
            throw new Volo.Abp.UserFriendlyException(L["ClaimTask:InvalidStatus"].Value);

        // Chuyển trạng thái Claim về Draft khi từ chối phân công
        var claim = await ClaimRepository.FirstOrDefaultAsync(c => c.Id == workTask.BusinessKey);
        if (claim != null)
        {
            claim.UpdateStatus(ClaimStatus.Draft);
            await ClaimRepository.UpdateAsync(claim);
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

    public virtual async Task TransferAsync(Guid workTaskId, TransferClaimTaskInput input)
    {
        var currentEmployeeId = await GetCurrentEmployeeIdAsync();
        var workTask = await WorkTaskRepository.GetAsync(workTaskId);
        if (workTask.BusinessCode != "CLAIM_ASSIGN" || workTask.AssigneeId != currentEmployeeId)
            throw new Volo.Abp.UserFriendlyException(L["ClaimTask:NotAuthorized"].Value);
        if (workTask.Status != WorkTaskStatus.New)
            throw new Volo.Abp.UserFriendlyException(L["ClaimTask:InvalidStatus"].Value);
        if (input.AssigneeId == currentEmployeeId)
            throw new Volo.Abp.UserFriendlyException("Người điều chuyển phải khác người đang xử lý.");

        var newAssignee = await EmployeeRepository.GetAsync(input.AssigneeId);
        var claim = await ClaimRepository.FirstOrDefaultAsync(c => c.Id == workTask.BusinessKey);
        if (claim != null)
        {
            claim.UpdateProcessEmpId(input.AssigneeId);
            await ClaimRepository.UpdateAsync(claim);
        }

        workTask.UpdateRejectionReason(input.ReasonId, input.ReasonDescription?.Trim());
        workTask.UpdateStatus(WorkTaskStatus.Transfer);
        workTask.UpdateActualDates(workTask.ActualStartDate, Clock.Now);
        await WorkTaskRepository.UpdateAsync(workTask);

        //var newTask = new WorkTask(
        //    GuidGenerator.Create(),
        //    workTask.WorkInstanceId,
        //    workTask.BusinessCode,
        //    workTask.BusinessName,
        //    workTask.BusinessKey,
        //    workTask.FormKey,
        //    Guid.NewGuid().ToString("N"),
        //    workTask.EventName,
        //    workTask.Name,
        //    workTask.Description,
        //    currentEmployeeId,
        //    workTask.BusinessAuthorityCode,
        //    input.AssigneeId,
        //    newAssignee.DepartmentId,
        //    newAssignee.OrgId,
        //    workTask.ResBusinessAssigneeId,
        //    WorkTaskStatus.New,
        //    workTask.Priority,
        //    workTask.TaskCategoryId,
        //    workTask.StartDate,
        //    workTask.EndDate,
        //    actualStartDate: null,
        //    actualEndDate: null,
        //    reasonId: null,
        //    reasonDescription: null);
        //await WorkTaskRepository.InsertAsync(newTask, autoSave: true);

        if (workTask.WorkInstanceId != null)
        {
            var workInstance = await _workInstanceRepository.GetAsync(workTask.WorkInstanceId.Value);
            var eventName = workTask.EventName ?? workInstance.WorkflowInstanceId;
            await _elsaWorkflowService.TriggerApprovalAsync(eventName, workInstance.WorkflowInstanceId, "next", input.AssigneeId.ToString());
        }
    }

    public virtual async Task AcceptAsync(Guid workTaskId)
    {
        var currentEmployeeId = await GetCurrentEmployeeIdAsync();
        var workTask = await WorkTaskRepository.GetAsync(workTaskId);
        if (workTask.BusinessCode != "CLAIM_ASSIGN" || workTask.AssigneeId != currentEmployeeId)
            throw new Volo.Abp.UserFriendlyException(L["ClaimTask:NotAuthorized"].Value);
        if (workTask.Status != WorkTaskStatus.New)
            throw new Volo.Abp.UserFriendlyException(L["ClaimTask:InvalidStatus"].Value);

        workTask.UpdateStatus(WorkTaskStatus.InProgress);
        workTask.UpdateActualDates(Clock.Now, null);
        await WorkTaskRepository.UpdateAsync(workTask);

        // Chuyển trạng thái Claim về Draft khi từ chối phân công
        var claim = await ClaimRepository.FirstOrDefaultAsync(c => c.Id == workTask.BusinessKey);
        if (claim != null)
        {
            claim.UpdateStatus(ClaimStatus.InProgress);
            await ClaimRepository.UpdateAsync(claim);
        }

        if (workTask.WorkInstanceId != null)
        {
            var workInstance = await _workInstanceRepository.GetAsync(workTask.WorkInstanceId.Value);
            var eventName = workTask.EventName ?? workInstance.WorkflowInstanceId;
            await _elsaWorkflowService.TriggerApprovalAsync(eventName, workInstance.WorkflowInstanceId, "approve", null);
        }
    }

    public virtual async Task AssignOnsiteAssessmentAsync(Guid claimId, AssignOnsiteAssessmentInput input)
    {
        var currentEmployeeId = await GetCurrentEmployeeIdAsync();
        var categoryId = await GetClaimAssignTaskCategoryIdAsync();
        if (!categoryId.HasValue)
        {
            throw new Volo.Abp.UserFriendlyException(L["ClaimTask:InvalidStatus"].Value);
        }

        var workTaskQuery = await WorkTaskRepository.GetQueryableAsync();
        var canAssign = await AsyncExecuter.AnyAsync(
            workTaskQuery.Where(w =>
                w.BusinessCode == "CLAIM_ASSIGN"
                && w.BusinessKey == claimId
                && w.TaskCategoryId == categoryId.Value
                && w.AssigneeId == currentEmployeeId
                && w.Status == WorkTaskStatus.InProgress));
        if (!canAssign)
        {
            throw new Volo.Abp.UserFriendlyException(L["ClaimTask:NotAuthorized"].Value);
        }

        await _onsiteAssessmentAssignmentService.CreateAsync(
            claimId,
            input.AssigneeOrganizationId,
            input.AssigneeId,
            input.StartDate,
            input.EndDate);
    }

    public virtual async Task OpenFolderAsync(Guid workTaskId, CreateClaimFolderDto input)
    {
        var currentEmployeeId = await GetCurrentEmployeeIdAsync();
        var workTask = await WorkTaskRepository.GetAsync(workTaskId);
        if (workTask.BusinessCode != "CLAIM_ASSIGN" || workTask.AssigneeId != currentEmployeeId)
        {
            throw new Volo.Abp.UserFriendlyException(L["ClaimTask:NotAuthorized"].Value);
        }
        if (workTask.Status != WorkTaskStatus.InProgress)
        {
            throw new Volo.Abp.UserFriendlyException(L["ClaimTask:InvalidStatus"].Value);
        }

        // Ensure claim id on DTO matches work task business key
        if (input.ClaimId == Guid.Empty)
        {
            input.ClaimId = workTask.BusinessKey;
        }

        if (input.ClaimId != workTask.BusinessKey)
        {
            throw new Volo.Abp.UserFriendlyException(L["ClaimTask:InvalidStatus"].Value);
        }

        await _claimFolderAppService.CreateAsync(input);
    }

    public virtual async Task<List<ClaimObjectTypeDto>> GetObjectTypesByPolicyAndProductAsync(
        Guid policyId,
        Guid productId,
        DateTime? incidentDate)
    {
        if (policyId == Guid.Empty || productId == Guid.Empty)
        {
            return new List<ClaimObjectTypeDto>();
        }

        var pvQuery = await PolicyVersionRepository.GetQueryableAsync();
        var versionQuery = pvQuery.Where(pv => pv.PolicyId == policyId && !pv.IsDeleted);
        if (incidentDate.HasValue)
        {
            versionQuery = versionQuery.Where(v =>
                v.EffectDate <= incidentDate.Value && v.ExpireDate >= incidentDate.Value);
        }

        var selectedVersionId = await AsyncExecuter.FirstOrDefaultAsync(
            versionQuery.OrderByDescending(v => v.Version).Select(v => v.Id));

        if (selectedVersionId == Guid.Empty)
        {
            return new List<ClaimObjectTypeDto>();
        }

        var ppQuery = await PolicyProductRepository.GetQueryableAsync();
        var policyProductId = await AsyncExecuter.FirstOrDefaultAsync(
            ppQuery
                .Where(pp =>
                    pp.PolicyVersionId == selectedVersionId && pp.ProductId == productId && !pp.IsDeleted)
                .Select(pp => pp.Id));

        if (policyProductId == Guid.Empty)
        {
            return new List<ClaimObjectTypeDto>();
        }

        var pcQuery = await PolicyCoverageRepository.GetQueryableAsync();
        var coverageIds = await AsyncExecuter.ToListAsync(
            pcQuery
                .Where(pc => pc.PolicyProductId == policyProductId && !pc.IsDeleted)
                .Select(pc => pc.CoverageId));

        if (coverageIds.Count == 0)
        {
            return new List<ClaimObjectTypeDto>();
        }

        var covQuery = await ProCoverageRepository.GetQueryableAsync();
        var objectTypeIds = await AsyncExecuter.ToListAsync(
            covQuery
                .Where(c =>
                    coverageIds.Contains(c.Id)
                    && c.ObjectTypeId != null
                    && c.ObjectTypeId != Guid.Empty)
                .Select(c => c.ObjectTypeId!.Value)
                .Distinct());

        if (objectTypeIds.Count == 0)
        {
            return new List<ClaimObjectTypeDto>();
        }

        var otQuery = await ResObjectTypeRepository.GetQueryableAsync();
        return await AsyncExecuter.ToListAsync(
            otQuery
                .Where(ot => objectTypeIds.Contains(ot.Id))
                .OrderBy(ot => ot.Name)
                .Select(ot => new ClaimObjectTypeDto
                {
                    Id = ot.Id,
                    Name = ot.Name,
                    Code = ot.Code
                }));
    }
}
