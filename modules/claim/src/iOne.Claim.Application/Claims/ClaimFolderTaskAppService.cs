using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using iOne.ClaimFolderExposureEstimates;
using iOne.ClaimFolderQuotationApprovals;
using iOne.ClaimFolders;
using iOne.ClaimIncidentRiskMotors;
using iOne.ClaimIncidents;
using iOne.Claims;
using iOne.HrDepartments;
using iOne.HrEmployees;
using iOne.ProProducts;
using iOne.ResTaskCategories;
using iOne.WorkTasks;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;
using ClaimEntity = iOne.Claims.Claim;

namespace iOne.Claim.Claims;

public class ClaimFolderTaskAppService : ApplicationService, IClaimFolderTaskAppService
{
    private sealed class AssignedTaskRow
    {
        public Guid Id { get; init; }
        public Guid ClaimFolderId { get; init; }
        public WorkTaskStatus Status { get; init; }
        public Guid ReporterId { get; init; }
        public DateTime CreationTime { get; init; }
        public bool IsReporter { get; init; }
        public bool IsAssignee { get; init; }
    }

    private sealed class FilteredTaskRow
    {
        public required AssignedTaskRow Task { get; init; }
        public required ClaimFolder ClaimFolder { get; init; }
        public required ClaimEntity Claim { get; init; }
    }

    protected IRepository<WorkTask, Guid> WorkTaskRepository { get; }
    protected IRepository<ClaimEntity, Guid> ClaimRepository { get; }
    protected IRepository<ClaimFolder, Guid> ClaimFolderRepository { get; }
    protected IRepository<ClaimFolderExposureEstimate, Guid> ClaimFolderExposureEstimateRepository { get; }
    protected IRepository<ClaimFolderQuotationApproval, Guid> QuotationApprovalRepository { get; }
    protected IRepository<ClaimIncident, Guid> ClaimIncidentRepository { get; }
    protected IRepository<ClaimIncidentRiskMotor, Guid> ClaimIncidentRiskMotorRepository { get; }
    protected IRepository<ProProduct, Guid> ProductRepository { get; }
    protected IRepository<HrEmployee, Guid> EmployeeRepository { get; }
    protected IRepository<HrDepartment, Guid> DepartmentRepository { get; }
    protected IRepository<ResTaskCategory, Guid> TaskCategoryRepository { get; }

    public ClaimFolderTaskAppService(
        IRepository<WorkTask, Guid> workTaskRepository,
        IRepository<ClaimEntity, Guid> claimRepository,
        IRepository<ClaimFolder, Guid> claimFolderRepository,
        IRepository<ClaimFolderExposureEstimate, Guid> claimFolderExposureEstimateRepository,
        IRepository<ClaimFolderQuotationApproval, Guid> quotationApprovalRepository,
        IRepository<ClaimIncident, Guid> claimIncidentRepository,
        IRepository<ClaimIncidentRiskMotor, Guid> claimIncidentRiskMotorRepository,
        IRepository<ProProduct, Guid> productRepository,
        IRepository<HrEmployee, Guid> employeeRepository,
        IRepository<HrDepartment, Guid> departmentRepository,
        IRepository<ResTaskCategory, Guid> taskCategoryRepository)
    {
        WorkTaskRepository = workTaskRepository;
        ClaimRepository = claimRepository;
        ClaimFolderRepository = claimFolderRepository;
        ClaimFolderExposureEstimateRepository = claimFolderExposureEstimateRepository;
        QuotationApprovalRepository = quotationApprovalRepository;
        ClaimIncidentRepository = claimIncidentRepository;
        ClaimIncidentRiskMotorRepository = claimIncidentRiskMotorRepository;
        ProductRepository = productRepository;
        EmployeeRepository = employeeRepository;
        DepartmentRepository = departmentRepository;
        TaskCategoryRepository = taskCategoryRepository;
    }

    public virtual async Task<PagedResultDto<ClaimTaskDto>> GetListAsync(GetClaimTasksInput input)
    {
        var currentEmployeeId = await GetCurrentEmployeeIdAsync();
        var detailAssessmentTaskCategoryId = await GetDetailAssessmentTaskCategoryIdAsync();
        if (!detailAssessmentTaskCategoryId.HasValue)
        {
            return new PagedResultDto<ClaimTaskDto>(0, new List<ClaimTaskDto>());
        }

        var workTaskQuery = await WorkTaskRepository.GetQueryableAsync();
        workTaskQuery = workTaskQuery
            .Where(wt => wt.BusinessCode == "CLAIM_DETAIL_ASSESSMENT"
                && wt.TaskCategoryId == detailAssessmentTaskCategoryId.Value
                && (!input.WorkTaskStatus.HasValue || wt.Status == input.WorkTaskStatus.Value)
                && (
                    wt.ReporterId == currentEmployeeId
                    || (
                        wt.AssigneeId == currentEmployeeId
                        && wt.Status != WorkTaskStatus.Rejected
                        && wt.Status != WorkTaskStatus.Cancelled
                        && wt.Status != WorkTaskStatus.Pending
                        && wt.Status != WorkTaskStatus.Return
                    )
                ));

        var assignedTasks = await AsyncExecuter.ToListAsync(
            workTaskQuery.Select(wt => new AssignedTaskRow
            {
                Id = wt.Id,
                ClaimFolderId = wt.BusinessKey,
                Status = wt.Status,
                ReporterId = wt.ReporterId,
                CreationTime = wt.CreationTime,
                IsReporter = wt.ReporterId == currentEmployeeId,
                IsAssignee = wt.AssigneeId == currentEmployeeId
            }));
        var assignedFolderIds = assignedTasks.Select(x => x.ClaimFolderId).ToList();
        if (assignedFolderIds.Count == 0)
        {
            return new PagedResultDto<ClaimTaskDto>(0, new List<ClaimTaskDto>());
        }

        var claimFolderQuery = await ClaimFolderRepository.GetQueryableAsync();
        var assignedFolders = await AsyncExecuter.ToListAsync(
            claimFolderQuery
                .Include(cf => cf.Insurer)
                .Where(cf => assignedFolderIds.Contains(cf.Id)));
        if (assignedFolders.Count == 0)
        {
            return new PagedResultDto<ClaimTaskDto>(0, new List<ClaimTaskDto>());
        }

        var blockedReassignFolderIds = await GetBlockedReassignFolderIdsAsync(assignedFolderIds);

        var assignedClaimIds = assignedFolders.Select(x => x.ClaimId).Distinct().ToList();

        var claimQuery = await ClaimRepository.GetQueryableAsync();
        claimQuery = claimQuery
            .Include(c => c.Lob)
            .Include(c => c.Insurer)
            .Include(c => c.OpenEmployee)
            .Where(c => assignedClaimIds.Contains(c.Id));

        claimQuery = await ApplyClaimFiltersAsync(claimQuery, input);

        var filteredClaims = await AsyncExecuter.ToListAsync(claimQuery);
        var claimById = filteredClaims.ToDictionary(c => c.Id, c => c);
        var filteredTasks = ApplyTaskSorting((
            from task in assignedTasks
            join folder in assignedFolders on task.ClaimFolderId equals folder.Id
            where claimById.ContainsKey(folder.ClaimId)
                && (task.Status != WorkTaskStatus.Rejected || !blockedReassignFolderIds.Contains(folder.Id))
            select new FilteredTaskRow
            {
                Task = task,
                ClaimFolder = folder,
                Claim = claimById[folder.ClaimId]
            }
        ).ToList(), input.Sorting);

        var totalCount = filteredTasks.Count;
        var pagedTasks = filteredTasks
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount > 0 ? input.MaxResultCount : 10)
            .ToList();

        var claimIds = pagedTasks.Select(x => x.Claim.Id).Distinct().ToList();
        var pagedClaimFolderIds = pagedTasks.Select(x => x.ClaimFolder.Id).Distinct().ToList();
        var estimateAmountByFolderId = new Dictionary<Guid, decimal>();
        if (pagedClaimFolderIds.Count > 0)
        {
            var estimateAmountRows = await AsyncExecuter.ToListAsync(
                (await ClaimFolderExposureEstimateRepository.GetQueryableAsync())
                .Where(x => x.ClaimFolderId.HasValue
                    && pagedClaimFolderIds.Contains(x.ClaimFolderId.Value)
                    && x.Status == ClaimFolderExposureEstimateStatus.Active)
                .GroupBy(x => x.ClaimFolderId!.Value)
                .Select(g => new
                {
                    ClaimFolderId = g.Key,
                    Amount = g.Sum(x => x.Amount)
                }));
            estimateAmountByFolderId = estimateAmountRows.ToDictionary(x => x.ClaimFolderId, x => x.Amount);
        }
        var claimFolders = await AsyncExecuter.ToListAsync(
            claimFolderQuery.Where(cf => claimIds.Contains(cf.ClaimId)));
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
        var productById = products.ToDictionary(p => p.Id, p => p);
        var canUpdateCompletedDetailedAssessmentByFolderId =
            await GetCanUpdateCompletedDetailedAssessmentByFolderIdAsync(pagedClaimFolderIds);

        var dtos = new List<ClaimTaskDto>();
        foreach (var row in pagedTasks)
        {
            var claim = row.Claim;
            var wtPair = row.Task;
            var claimFolder = row.ClaimFolder;

            incidentByClaimId.TryGetValue(claim.Id, out var incident);
            ClaimIncidentRiskMotor? riskMotor = null;
            if (incident != null)
                riskMotorByIncidentId.TryGetValue(incident.Id, out riskMotor);
            ProProduct? product = null;
            if (claimFolder.ProductId != null)
                productById.TryGetValue(claimFolder.ProductId.Value, out product);

            // Đối tác BH gốc: ưu tiên theo HSBT (ClaimFolder), fallback Claim (yêu cầu) — Claim.InsurerId có thể trống sau khi mở hồ sơ.
            var folderInsurer = claimFolder.Insurer;
            var claimInsurer = claim.Insurer;
            dtos.Add(new ClaimTaskDto
            {
                Id = wtPair.Id,
                ClaimId = claim.Id,
                Code = claim.Code,
                FolderNo = claimFolder.FolderNo,
                InsurerName = folderInsurer?.Name ?? claimInsurer?.Name,
                InsurerCode = folderInsurer?.Code ?? claimInsurer?.Code,
                LobName = claim.Lob?.Name,
                ProductName = product?.Name,
                NotifierName = claim.NotifierName,
                OpenDate = claim.OpenDate,
                NotifyDate = claim.NotifyDate,
                CarPlate = riskMotor?.CarPlate,
                IncidentDate = incident?.IncidentDate,
                OnLocation = incident?.OnLocation,
                OpenEmployeeName = claim.OpenEmployee?.FullName,
                ProcessClaimType = claim.ProcessClaimType,
                ClaimStatus = claim.Status,
                WorkTaskStatus = wtPair.Status,
                TaskCreationTime = wtPair.CreationTime,
                EstimateAmount = estimateAmountByFolderId.GetValueOrDefault(claimFolder.Id),
                CanCancel = (wtPair.Status == WorkTaskStatus.New || wtPair.Status == WorkTaskStatus.Rejected)
                    && wtPair.ReporterId == currentEmployeeId,
                IsReporter = wtPair.IsReporter,
                IsAssignee = wtPair.IsAssignee,
                CanReassign = wtPair.IsReporter
                    && !blockedReassignFolderIds.Contains(claimFolder.Id)
                    && (wtPair.Status == WorkTaskStatus.Rejected
                        || wtPair.Status == WorkTaskStatus.Return),
                CanUpdateCompletedDetailedAssessment =
                    wtPair.Status == WorkTaskStatus.Completed
                    && wtPair.IsAssignee
                    && canUpdateCompletedDetailedAssessmentByFolderId.GetValueOrDefault(claimFolder.Id)
            });
        }

        return new PagedResultDto<ClaimTaskDto>(totalCount, dtos);
    }

    private async Task<HashSet<Guid>> GetBlockedReassignFolderIdsAsync(List<Guid> claimFolderIds)
    {
        if (claimFolderIds.Count == 0)
        {
            return new HashSet<Guid>();
        }

        var workTaskQuery = await WorkTaskRepository.GetQueryableAsync();
        var blockedIds = await AsyncExecuter.ToListAsync(
            workTaskQuery
                .Where(wt =>
                    claimFolderIds.Contains(wt.BusinessKey)
                    && wt.BusinessCode == "CLAIM_DETAIL_ASSESSMENT"
                    && wt.Status != WorkTaskStatus.Rejected
                    && wt.Status != WorkTaskStatus.Return)
                .Select(wt => wt.BusinessKey)
                .Distinct());

        return blockedIds.ToHashSet();
    }

    private async Task<Dictionary<Guid, bool>> GetCanUpdateCompletedDetailedAssessmentByFolderIdAsync(
        List<Guid> claimFolderIds)
    {
        if (claimFolderIds.Count == 0)
        {
            return new Dictionary<Guid, bool>();
        }

        var quotationApprovals = await AsyncExecuter.ToListAsync(
            (await QuotationApprovalRepository.GetQueryableAsync())
                .Where(x => x.ClaimFolderId.HasValue && claimFolderIds.Contains(x.ClaimFolderId.Value))
                .OrderByDescending(x => x.CreationTime));

        var canUpdateByFolderId = quotationApprovals
            .GroupBy(x => x.ClaimFolderId!.Value)
            .ToDictionary(
                g => g.Key,
                g =>
                {
                    var latest = g.First();
                    return latest.Status == ClaimFolderQuotationApprovalStatus.New
                        || latest.Status == ClaimFolderQuotationApprovalStatus.Rejected
                        || latest.Status == ClaimFolderQuotationApprovalStatus.Cancelled;
                });

        foreach (var claimFolderId in claimFolderIds)
        {
            if (!canUpdateByFolderId.ContainsKey(claimFolderId))
            {
                canUpdateByFolderId[claimFolderId] = true;
            }
        }

        return canUpdateByFolderId;
    }

    private async Task<Guid> GetCurrentEmployeeIdAsync()
    {
        if (CurrentUser.Id == null)
            throw new Volo.Abp.UserFriendlyException("UserNotAuthenticated");
        var employee = await EmployeeRepository.FirstOrDefaultAsync(e => e.UserId == CurrentUser.Id.Value);
        if (employee == null)
            throw new Volo.Abp.UserFriendlyException("EmployeeNotFoundForUser");
        return employee.Id;
    }

    private async Task<Guid?> GetDetailAssessmentTaskCategoryIdAsync()
    {
        var taskCategoryQuery = await TaskCategoryRepository.GetQueryableAsync();
        return await AsyncExecuter.FirstOrDefaultAsync(
            taskCategoryQuery
                .Where(x => x.Code == "CLAIM_DETAIL_ASSESSMENT_TASK")
                .Select(x => (Guid?)x.Id));
    }

    private async Task<IQueryable<ClaimEntity>> ApplyClaimFiltersAsync(IQueryable<ClaimEntity> query, GetClaimTasksInput input)
    {
        if (input.LobId.HasValue)
            query = query.Where(c => c.LobId == input.LobId.Value);
        if (input.InsurerId.HasValue)
            query = query.Where(c => c.InsurerId == input.InsurerId.Value);
        if (input.ProcessClaimType.HasValue)
            query = query.Where(c => c.ProcessClaimType == input.ProcessClaimType.Value);
        var filterClaimStatus = input.Status ?? input.ClaimStatus;
        if (filterClaimStatus.HasValue)
            query = query.Where(c => c.Status == filterClaimStatus.Value);
        if (!string.IsNullOrWhiteSpace(input.NotifierPhone))
            query = query.Where(c => c.NotifierPhone != null && c.NotifierPhone.Contains(input.NotifierPhone));
        if (input.OpenDateFrom.HasValue)
            query = query.Where(c => c.OpenDate >= input.OpenDateFrom.Value.Date);
        if (input.OpenDateTo.HasValue)
            query = query.Where(c => c.OpenDate <= input.OpenDateTo.Value.Date.AddDays(1).AddTicks(-1));
        if (!string.IsNullOrWhiteSpace(input.Code))
            query = query.Where(c => c.Code != null && c.Code.Contains(input.Code));

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
                    var normalizedValue = item.RiskMotor != null && !string.IsNullOrEmpty(item.RiskMotor.CarPlate) ? NormalizeCarInfo(item.RiskMotor.CarPlate) : string.Empty;
                    if (normalizedValue != normalizedInput) match = false;
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

    private static List<FilteredTaskRow> ApplyTaskSorting(List<FilteredTaskRow> tasks, string? sorting)
    {
        var sortText = string.IsNullOrWhiteSpace(sorting) ? "creationTime desc" : sorting;
        var parts = sortText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var field = parts.Length > 0 ? parts[0].ToLowerInvariant() : "creationtime";
        var order = parts.Length > 1 && parts[1].ToLowerInvariant() == "asc" ? "asc" : "desc";

        return field switch
        {
            "code" => order == "asc"
                ? tasks.OrderBy(x => x.Claim.Code).ToList()
                : tasks.OrderByDescending(x => x.Claim.Code).ToList(),
            "opendate" => order == "asc"
                ? tasks.OrderBy(x => x.Claim.OpenDate).ToList()
                : tasks.OrderByDescending(x => x.Claim.OpenDate).ToList(),
            "notifydate" => order == "asc"
                ? tasks.OrderBy(x => x.Claim.NotifyDate).ToList()
                : tasks.OrderByDescending(x => x.Claim.NotifyDate).ToList(),
            "claimstatus" => order == "asc"
                ? tasks.OrderBy(x => x.Claim.Status).ToList()
                : tasks.OrderByDescending(x => x.Claim.Status).ToList(),
            "worktaskstatus" => order == "asc"
                ? tasks.OrderBy(x => x.Task.Status).ToList()
                : tasks.OrderByDescending(x => x.Task.Status).ToList(),
            "creationtime" => order == "asc"
                ? tasks.OrderBy(x => x.Task.CreationTime).ToList()
                : tasks.OrderByDescending(x => x.Task.CreationTime).ToList(),
            _ => tasks.OrderByDescending(x => x.Task.CreationTime).ToList()
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
}
