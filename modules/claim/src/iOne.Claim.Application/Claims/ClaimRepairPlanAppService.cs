using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using iOne.Claim.Localization;
using iOne.ClaimFolderIncidentObjects;
using iOne.ClaimFolderItemPlans;
using iOne.ClaimFolderItems;
using iOne.ClaimFolders;
using iOne.ClaimFolderQuotationApprovals;
using iOne.HrEmployees;
using iOne.ProProducts;
using iOne.ProProductTypes;
using iOne.ResBusinessAssignees;
using iOne.ResBusinessAuthorities;
using iOne.ResObjectTypeItems;
using iOne.ResClaimPlans;
using iOne.ResObjectTypes;
using iOne.ResPartners;
using iOne.WorkInstances;
using iOne.Workflow;
using iOne.WorkTasks;
using iOne.Claims;
using iOne.ClaimIncidents;
using iOne.ClaimIncidentRiskMotors;
using iOne.HrDepartments;
using iOne.ProLineOfBusinesses;
using iOne.Claim.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;
using ClaimEntity = iOne.Claims.Claim;

namespace iOne.Claim.Claims;

public class ClaimRepairPlanAppService : ApplicationService, IClaimRepairPlanAppService
{
    private const string RepairPlanApprovalBusinessCode = "CLAIM_QUOTAION_APPROVAL";
    private const string RepairPlanApprovalAuthorityCode = "APPROVAL_LEVEL_ONE";
    private const string DetailAssessmentBusinessCode = "CLAIM_DETAIL_ASSESSMENT";

    /// <summary>Mã loại sản phẩm VCX (bảo hiểm vật chất xe / nhóm DBV–VCX).</summary>
    private const string PascEligibleProductTypeCode = "VCX";

    private static readonly HashSet<string> RepairPlanCostTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "man_cost",
        "paint_cost",
        "material_cost",
    };

    private static readonly JsonSerializerOptions RepairPlanJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    protected IRepository<WorkTask, Guid> WorkTaskRepository { get; }
    protected IRepository<ClaimFolder, Guid> ClaimFolderRepository { get; }
    protected IRepository<ClaimFolderItem, Guid> ClaimFolderItemRepository { get; }
    protected IRepository<ClaimFolderItemPlan, Guid> ClaimFolderItemPlanRepository { get; }
    protected IRepository<ClaimFolderQuotationApproval, Guid> QuotationApprovalRepository { get; }
    protected IRepository<ResObjectTypeItem, Guid> ResObjectTypeItemRepository { get; }
    protected IRepository<ResClaimPlan, Guid> ResClaimPlanRepository { get; }
    protected IRepository<ResPartner, Guid> ResPartnerRepository { get; }
    protected IRepository<HrEmployee, Guid> EmployeeRepository { get; }
    protected IRepository<ClaimFolderIncidentObject, Guid> IncidentObjectRepository { get; }
    protected IRepository<ResObjectType, Guid> ResObjectTypeRepository { get; }
    protected IRepository<ProProduct, Guid> ProProductRepository { get; }
    protected IRepository<ProProductType, Guid> ProProductTypeRepository { get; }
    protected IRepository<ResBusinessAssignee, Guid> ResBusinessAssigneeRepository { get; }
    protected IRepository<ResBusinessAuthority, Guid> ResBusinessAuthorityRepository { get; }
    protected IElsaWorkflowService ElsaWorkflowService { get; }
    protected IRepository<ClaimEntity, Guid> ClaimRepository { get; }
    protected IRepository<ClaimIncident, Guid> ClaimIncidentRepository { get; }
    protected IRepository<ClaimIncidentRiskMotor, Guid> ClaimIncidentRiskMotorRepository { get; }
    protected IRepository<HrDepartment, Guid> DepartmentRepository { get; }
    protected IRepository<ProLineOfBusiness, Guid> LobRepository { get; }
    protected IWorkInstanceRepository WorkInstanceRepository { get; }

    public ClaimRepairPlanAppService(
        IRepository<WorkTask, Guid> workTaskRepository,
        IRepository<ClaimFolder, Guid> claimFolderRepository,
        IRepository<ClaimFolderItem, Guid> claimFolderItemRepository,
        IRepository<ClaimFolderItemPlan, Guid> claimFolderItemPlanRepository,
        IRepository<ClaimFolderQuotationApproval, Guid> quotationApprovalRepository,
        IRepository<ResObjectTypeItem, Guid> resObjectTypeItemRepository,
        IRepository<ResClaimPlan, Guid> resClaimPlanRepository,
        IRepository<ResPartner, Guid> resPartnerRepository,
        IRepository<HrEmployee, Guid> employeeRepository,
        IRepository<ClaimFolderIncidentObject, Guid> incidentObjectRepository,
        IRepository<ResObjectType, Guid> resObjectTypeRepository,
        IRepository<ProProduct, Guid> proProductRepository,
        IRepository<ProProductType, Guid> proProductTypeRepository,
        IRepository<ResBusinessAssignee, Guid> resBusinessAssigneeRepository,
        IRepository<ResBusinessAuthority, Guid> resBusinessAuthorityRepository,
        IRepository<ClaimEntity, Guid> claimRepository,
        IRepository<ClaimIncident, Guid> claimIncidentRepository,
        IRepository<ClaimIncidentRiskMotor, Guid> claimIncidentRiskMotorRepository,
        IRepository<HrDepartment, Guid> departmentRepository,
        IRepository<ProLineOfBusiness, Guid> lobRepository,
        IWorkInstanceRepository workInstanceRepository,
        IElsaWorkflowService elsaWorkflowService)
    {
        WorkTaskRepository = workTaskRepository;
        ClaimFolderRepository = claimFolderRepository;
        ClaimFolderItemRepository = claimFolderItemRepository;
        ClaimFolderItemPlanRepository = claimFolderItemPlanRepository;
        QuotationApprovalRepository = quotationApprovalRepository;
        ResObjectTypeItemRepository = resObjectTypeItemRepository;
        ResClaimPlanRepository = resClaimPlanRepository;
        ResPartnerRepository = resPartnerRepository;
        EmployeeRepository = employeeRepository;
        IncidentObjectRepository = incidentObjectRepository;
        ResObjectTypeRepository = resObjectTypeRepository;
        ProProductRepository = proProductRepository;
        ProProductTypeRepository = proProductTypeRepository;
        ResBusinessAssigneeRepository = resBusinessAssigneeRepository;
        ResBusinessAuthorityRepository = resBusinessAuthorityRepository;
        ClaimRepository = claimRepository;
        ClaimIncidentRepository = claimIncidentRepository;
        ClaimIncidentRiskMotorRepository = claimIncidentRiskMotorRepository;
        DepartmentRepository = departmentRepository;
        LobRepository = lobRepository;
        WorkInstanceRepository = workInstanceRepository;
        ElsaWorkflowService = elsaWorkflowService;
        LocalizationResource = typeof(ClaimResource);
    }

    // ──────────────────────────────────────────────────────────
    // GET INIT DATA
    // ──────────────────────────────────────────────────────────

    public virtual async Task<RepairPlanInitDataDto> GetInitDataAsync(Guid claimId, Guid workTaskId)
    {
        var workTask = await AsyncExecuter.FirstOrDefaultAsync(
            (await WorkTaskRepository.GetQueryableAsync())
            .Where(x => x.Id == workTaskId)
            .Select(x => new { x.Id, x.BusinessCode, x.BusinessKey }));

        if (workTask == null)
        {
            throw new Volo.Abp.UserFriendlyException("Không tìm thấy công việc.");
        }

        string? savedDataJson;
        Guid? folderId = null;
        string? quotationApprovalStatus = null;

        if (workTask.BusinessCode == RepairPlanApprovalBusinessCode)
        {
            // BusinessKey = ClaimFolderQuotationApproval.Id (luồng phê duyệt PASC)
            var quotationApproval = await QuotationApprovalRepository.GetAsync(workTask.BusinessKey);
            if (quotationApproval.ClaimId != claimId)
            {
                throw new Volo.Abp.UserFriendlyException("Không khớp yêu cầu bồi thường.");
            }

            quotationApprovalStatus = ClaimFolderQuotationApprovalStatusDatabase.ToColumnValue(quotationApproval.Status);
            savedDataJson = quotationApproval.Data;

            if (quotationApproval.ClaimFolderId.HasValue)
            {
                folderId = await AsyncExecuter.FirstOrDefaultAsync(
                    (await ClaimFolderRepository.GetQueryableAsync())
                        .Where(x => x.Id == quotationApproval.ClaimFolderId.Value && x.ClaimId == claimId)
                        .Select(x => (Guid?)x.Id));
            }
            else
            {
                folderId = await AsyncExecuter.FirstOrDefaultAsync(
                    (await ClaimFolderRepository.GetQueryableAsync())
                        .Where(x => x.ClaimId == claimId)
                        .OrderByDescending(x => x.CreationTime)
                        .Select(x => (Guid?)x.Id));
            }
        }
        else if (workTask.BusinessCode == DetailAssessmentBusinessCode)
        {
            var claimFolderId = await ResolveClaimFolderIdFromAssessmentWorkTaskAsync(claimId, workTaskId);

            folderId = await AsyncExecuter.FirstOrDefaultAsync(
                (await ClaimFolderRepository.GetQueryableAsync())
                    .Where(x => x.Id == claimFolderId && x.ClaimId == claimId)
                    .Select(x => (Guid?)x.Id));

            var displayable = await FindLatestDisplayableQuotationApprovalAsync(claimId, claimFolderId);
            savedDataJson = displayable?.Data;
            if (savedDataJson == null)
            {
                var draftFallback = await FindExistingDraftAsync(claimId, claimFolderId);
                savedDataJson = draftFallback?.Data;
            }

            var statusFolderId = folderId ?? claimFolderId;
            quotationApprovalStatus = await ResolveInitQuotationApprovalStatusForFolderAsync(claimId, statusFolderId);
        }
        else
        {
            // Các loại công việc khác: giữ hành vi cũ (BusinessKey là folder hoặc fallback theo claim)
            var claimFolderId = workTask.BusinessKey;
            folderId = await AsyncExecuter.FirstOrDefaultAsync(
                (await ClaimFolderRepository.GetQueryableAsync())
                    .Where(x => x.Id == claimFolderId || x.ClaimId == claimId)
                    .OrderByDescending(x => x.CreationTime)
                    .Select(x => (Guid?)x.Id));

            savedDataJson = folderId.HasValue
                ? (await FindLatestDisplayableQuotationApprovalAsync(claimId, folderId.Value))?.Data
                : null;

            quotationApprovalStatus = await ResolveInitQuotationApprovalStatusForFolderAsync(claimId, folderId);
        }

        var isPascProductEligible = folderId.HasValue && await IsPascEligibleForFolderAsync(folderId.Value);

        List<RepairPlanAssessmentItemDto> assessmentItems;
        List<RepairPlanIncidentObjectDto> incidentObjects;
        string? effectiveSavedData = savedDataJson;
        string? effectiveQuotationApprovalStatus = quotationApprovalStatus;

        if (!isPascProductEligible)
        {
            assessmentItems = new List<RepairPlanAssessmentItemDto>();
            incidentObjects = new List<RepairPlanIncidentObjectDto>();
            effectiveSavedData = null;
            effectiveQuotationApprovalStatus = null;
        }
        else
        {
            assessmentItems = folderId.HasValue
                ? await BuildAssessmentItemsAsync(folderId.Value)
                : new List<RepairPlanAssessmentItemDto>();

            incidentObjects = folderId.HasValue
                ? await BuildIncidentObjectsAsync(folderId.Value)
                : new List<RepairPlanIncidentObjectDto>();
        }

        return new RepairPlanInitDataDto
        {
            ObjectTypes = new List<RepairPlanObjectTypeDto>(),
            Items = assessmentItems,
            IncidentObjects = incidentObjects,
            SavedData = effectiveSavedData,
            QuotationApprovalStatus = effectiveQuotationApprovalStatus,
            IsPascProductEligible = isPascProductEligible,
        };
    }

    public virtual async Task<RepairPlanPascEligibilityDto> GetPascEligibilityAsync(Guid claimId, Guid workTaskId)
    {
        var folderId = await TryResolveRepairPlanInitFolderIdAsync(claimId, workTaskId);
        var eligible = folderId.HasValue && await IsPascEligibleForFolderAsync(folderId.Value);
        return new RepairPlanPascEligibilityDto { Eligible = eligible };
    }

    // ──────────────────────────────────────────────────────────
    // GET GARAGES
    // ──────────────────────────────────────────────────────────

    public virtual async Task<List<RepairPlanGarageOptionDto>> GetGaragesAsync(string? search, int maxResultCount = 50)
    {
        var query = (await ResPartnerRepository.GetQueryableAsync())
            .Where(x => x.PartnerRole == "GARAGE")
            .OrderBy(x => x.Name);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalized = search.Trim().ToLower();
            query = (IOrderedQueryable<ResPartner>)query
                .Where(x => x.Name.ToLower().Contains(normalized));
        }

        var items = await AsyncExecuter.ToListAsync(
            query
            .Take(maxResultCount)
            .Select(x => new RepairPlanGarageOptionDto
            {
                Id = x.Id,
                Name = x.Name,
            }));

        return items;
    }

    // ──────────────────────────────────────────────────────────
    // SUBMIT INFO (popup)
    // ──────────────────────────────────────────────────────────

    public virtual async Task<RepairPlanSubmitInfoDto> GetSubmitInfoAsync(Guid claimId, Guid workTaskId)
    {
        var claimFolderId = await ResolveClaimFolderIdFromAssessmentWorkTaskAsync(claimId, workTaskId);

        if (!await IsPascEligibleForFolderAsync(claimFolderId))
        {
            throw new Volo.Abp.UserFriendlyException(PascNotEligibleMessage);
        }

        var folderRow = await AsyncExecuter.FirstOrDefaultAsync(
            (await ClaimFolderRepository.GetQueryableAsync())
            .Where(x => x.Id == claimFolderId)
            .Select(x => new { x.FolderNo, x.ProductId }));

        if (folderRow == null)
        {
            throw new Volo.Abp.UserFriendlyException("Không tìm thấy hồ sơ bồi thường.");
        }

        var productName = string.Empty;
        if (folderRow.ProductId.HasValue)
        {
            productName = await AsyncExecuter.FirstOrDefaultAsync(
                (await ProProductRepository.GetQueryableAsync())
                .Where(p => p.Id == folderRow.ProductId.Value && !p.IsDeleted)
                .Select(p => p.Name)) ?? string.Empty;
        }

        var currentEmployeeId = await GetCurrentEmployeeIdAsync();
        var submitterName = await AsyncExecuter.FirstOrDefaultAsync(
            (await EmployeeRepository.GetQueryableAsync())
            .Where(e => e.Id == currentEmployeeId && !e.IsDeleted)
            .Select(e => e.FullName)) ?? string.Empty;

        var authorityName = await AsyncExecuter.FirstOrDefaultAsync(
            (await ResBusinessAuthorityRepository.GetQueryableAsync())
            .Where(a => a.BusinessCode == RepairPlanApprovalBusinessCode
                        && a.Code == RepairPlanApprovalAuthorityCode
                        && a.Status == ResBusinessAuthorityStatus.Active
                        && !a.IsDeleted)
            .Select(a => a.Name)) ?? string.Empty;

        var now = Clock.Now;
        var assigneeQuery = await ResBusinessAssigneeRepository.GetQueryableAsync();
        var employeeQuery = await EmployeeRepository.GetQueryableAsync();

        var approverRows = await AsyncExecuter.ToListAsync(
            from ra in assigneeQuery
            join e in employeeQuery on ra.AssigneeId equals e.Id
            where ra.BusinessCode == RepairPlanApprovalBusinessCode
                  && ra.AuthorityCode == RepairPlanApprovalAuthorityCode
                  && ra.AssigneeType == ResBusinessAssigneeType.Emp
                  && ra.Status == ResBusinessAssigneeStatus.Active
                  && ra.EffectDate <= now
                  && (ra.ExpireDate == null || ra.ExpireDate >= now)
                  && !ra.IsDeleted
                  && ra.AssigneeId != null
                  && !e.IsDeleted
            orderby e.FullName
            select new RepairPlanApproverDto
            {
                Id = e.Id,
                Name = e.FullName,
            });

        Guid? suggested = null;
        if (approverRows.Count > 0)
        {
            suggested = approverRows[Random.Shared.Next(approverRows.Count)].Id;
        }

        return new RepairPlanSubmitInfoDto
        {
            SubmitterName = submitterName,
            FolderNo = folderRow.FolderNo,
            ProductName = productName,
            BusinessAuthorityName = authorityName,
            Approvers = approverRows,
            SuggestedApproverId = suggested,
        };
    }

    // ──────────────────────────────────────────────────────────
    // SAVE (draft)
    // ──────────────────────────────────────────────────────────

    public virtual async Task<RepairPlanSavedDto> SaveAsync(SaveRepairPlanInput input)
    {
        var currentEmployeeId = await GetCurrentEmployeeIdAsync();
        ValidateSaveInput(input);

        var resolvedFolderId = await ResolveClaimFolderIdForSaveAsync(input);
        var folderForPascCheck = resolvedFolderId ?? input.ClaimFolderId;
        if (folderForPascCheck.HasValue && !await IsPascEligibleForFolderAsync(folderForPascCheck.Value))
        {
            throw new Volo.Abp.UserFriendlyException(PascNotEligibleMessage);
        }

        var existing = await FindExistingDraftAsync(input.ClaimId, resolvedFolderId);

        if (existing != null)
        {
            existing.UpdatePartnerId(input.PartnerId);
            existing.UpdateAmounts(
                input.ClaimAmount,
                input.DiscountAmount,
                input.DepreciationAmount,
                input.ExpenseAmount,
                input.AssessmentAmount,
                input.LossPreventionAmount,
                input.RescueAmount,
                input.OtherAmount);
            existing.UpdateData(input.Data);
            if (resolvedFolderId.HasValue)
            {
                existing.UpdateClaimFolderId(resolvedFolderId);
            }
            else if (input.ClaimFolderId.HasValue)
            {
                existing.UpdateClaimFolderId(input.ClaimFolderId);
            }

            await QuotationApprovalRepository.UpdateAsync(existing, autoSave: true);
            await SyncClaimFolderItemPlansFromRepairDataAsync(
                resolvedFolderId ?? input.ClaimFolderId,
                input.PartnerId,
                input.Data);
            return new RepairPlanSavedDto { Id = existing.Id, Status = existing.Status.ToString() };
        }

        var folderForNew = resolvedFolderId ?? input.ClaimFolderId;

        var entity = new ClaimFolderQuotationApproval(
            GuidGenerator.Create(),
            input.ClaimId,
            currentEmployeeId,
            Clock.Now,
            ClaimFolderQuotationApprovalStatus.New,
            folderForNew,
            input.PartnerId,
            input.ClaimAmount,
            input.DiscountAmount,
            input.DepreciationAmount,
            input.ExpenseAmount,
            input.AssessmentAmount,
            input.LossPreventionAmount,
            input.RescueAmount,
            input.OtherAmount,
            input.Data);

        await QuotationApprovalRepository.InsertAsync(entity, autoSave: true);
        await SyncClaimFolderItemPlansFromRepairDataAsync(
            folderForNew,
            input.PartnerId,
            input.Data);
        return new RepairPlanSavedDto { Id = entity.Id, Status = "new" };
    }

    // ──────────────────────────────────────────────────────────
    // SUBMIT (new)
    // ──────────────────────────────────────────────────────────

    public virtual async Task<RepairPlanSavedDto> SubmitAsync(SaveRepairPlanInput input)
    {
        if (!input.WorkTaskId.HasValue || input.WorkTaskId.Value == Guid.Empty)
        {
            throw new Volo.Abp.UserFriendlyException("WorkTaskId là bắt buộc khi trình duyệt.");
        }

        if (!input.ApproverId.HasValue || input.ApproverId.Value == Guid.Empty)
        {
            throw new Volo.Abp.UserFriendlyException("Vui lòng chọn người duyệt.");
        }

        var claimFolderId = await ResolveClaimFolderIdFromAssessmentWorkTaskAsync(input.ClaimId, input.WorkTaskId.Value);

        if (!await IsPascEligibleForFolderAsync(claimFolderId))
        {
            throw new Volo.Abp.UserFriendlyException(PascNotEligibleMessage);
        }

        var approverExists = await AsyncExecuter.AnyAsync(
            (await EmployeeRepository.GetQueryableAsync())
            .Where(e => e.Id == input.ApproverId.Value && !e.IsDeleted));
        if (!approverExists)
        {
            throw new Volo.Abp.UserFriendlyException("Người duyệt không hợp lệ.");
        }

        var currentEmployeeId = await GetCurrentEmployeeIdAsync();
        ValidateSaveInput(input);

        await EnsureCanSubmitQuotationApprovalAsync(input.ClaimId, claimFolderId);

        var submittedDate = input.SubmittedDate ?? Clock.Now;
        var existing = await FindExistingDraftAsync(input.ClaimId, claimFolderId);

        ClaimFolderQuotationApproval savedEntity;

        if (existing != null)
        {
            existing.UpdatePartnerId(input.PartnerId);
            existing.UpdateAmounts(
                input.ClaimAmount,
                input.DiscountAmount,
                input.DepreciationAmount,
                input.ExpenseAmount,
                input.AssessmentAmount,
                input.LossPreventionAmount,
                input.RescueAmount,
                input.OtherAmount);
            existing.UpdateData(input.Data);
            existing.UpdateStatus(ClaimFolderQuotationApprovalStatus.Pending_Approval);
            existing.UpdateClaimFolderId(claimFolderId);
            existing.UpdateSubmission(currentEmployeeId, submittedDate);
            existing.UpdateApproverId(input.ApproverId);
            existing.UpdateDescription(input.Description);

            await QuotationApprovalRepository.UpdateAsync(existing, autoSave: true);
            savedEntity = existing;
        }
        else
        {
            var entity = new ClaimFolderQuotationApproval(
                GuidGenerator.Create(),
                input.ClaimId,
                currentEmployeeId,
                submittedDate,
                ClaimFolderQuotationApprovalStatus.Pending_Approval,
                claimFolderId,
                input.PartnerId,
                input.ClaimAmount,
                input.DiscountAmount,
                input.DepreciationAmount,
                input.ExpenseAmount,
                input.AssessmentAmount,
                input.LossPreventionAmount,
                input.RescueAmount,
                input.OtherAmount,
                input.Data);
            entity.UpdateApproverId(input.ApproverId);
            entity.UpdateDescription(input.Description);

            await QuotationApprovalRepository.InsertAsync(entity, autoSave: true);
            savedEntity = entity;
        }

        await SyncClaimFolderItemPlansFromRepairDataAsync(claimFolderId, input.PartnerId, input.Data);

        await SyncRepairPlanApprovalWorkTaskAsync(savedEntity.Id, input.ApproverId.Value, input.Description);

        return new RepairPlanSavedDto { Id = savedEntity.Id, Status = "pending_approval" };
    }

    // ──────────────────────────────────────────────────────────
    // QUOTATION (PASC) APPROVAL — LIST / APPROVE / REJECT / REASSIGN
    // ──────────────────────────────────────────────────────────

    [Authorize(ClaimPermissions.QuotationApprovalList)]
    public virtual async Task<PagedResultDto<QuotationApprovalListRowDto>> GetApprovalListAsync(GetQuotationApprovalListInput input)
    {
        var currentEmployeeId = await GetCurrentEmployeeIdAsync();
        var wtQ = await WorkTaskRepository.GetQueryableAsync();
        var cfQ = await ClaimFolderRepository.GetQueryableAsync();
        var cq = await ClaimRepository.GetQueryableAsync();
        var qaQ = await QuotationApprovalRepository.GetQueryableAsync();
        var insQ = await ResPartnerRepository.GetQueryableAsync();
        var lobQ = await LobRepository.GetQueryableAsync();
        var prodQ = await ProProductRepository.GetQueryableAsync();
        var empQ = await EmployeeRepository.GetQueryableAsync();

        var filtered =
            from wt in wtQ
            join qa in qaQ on wt.BusinessKey equals qa.Id
            join cf in cfQ on qa.ClaimFolderId equals cf.Id
            join c in cq on cf.ClaimId equals c.Id
            // Inbox only: Return/Transfer are workflow handoff — exclude so the previous assignee does not keep a ghost row after reassignment.
            where wt.BusinessCode == RepairPlanApprovalBusinessCode &&
                  wt.AssigneeId == currentEmployeeId &&
                  wt.Status != WorkTaskStatus.Return &&
                  wt.Status != WorkTaskStatus.Transfer &&
                  !qa.IsDeleted
            select new { wt, cf, c };

        if (input.LobId.HasValue)
        {
            filtered = filtered.Where(x => x.c.LobId == input.LobId.Value);
        }

        if (input.InsurerId.HasValue)
        {
            filtered = filtered.Where(x => x.c.InsurerId == input.InsurerId.Value);
        }

        if (input.ProcessDeptId.HasValue)
        {
            var deptIds = await GetDepartmentAndChildIdsAsync(input.ProcessDeptId.Value);
            filtered = filtered.Where(x => x.c.ProcessDeptId.HasValue && deptIds.Contains(x.c.ProcessDeptId.Value));
        }

        if (input.ClaimId.HasValue)
        {
            filtered = filtered.Where(x => x.c.Id == input.ClaimId.Value);
        }

        if (!string.IsNullOrWhiteSpace(input.FolderNo))
        {
            var upper = input.FolderNo.Trim().ToUpperInvariant();
            filtered = filtered.Where(x => x.cf.FolderNo.ToUpper() == upper);
        }

        if (input.ReporterId.HasValue)
        {
            filtered = filtered.Where(x => x.wt.ReporterId == input.ReporterId.Value);
        }

        if (!string.IsNullOrWhiteSpace(input.ApprovalStatus))
        {
            var bucket = input.ApprovalStatus.Trim().ToLowerInvariant();
            filtered = filtered.Where(x =>
                (bucket == "new" &&
                    x.wt.Status != WorkTaskStatus.Completed &&
                    x.wt.Status != WorkTaskStatus.Accepted &&
                    x.wt.Status != WorkTaskStatus.Approved &&
                    x.wt.Status != WorkTaskStatus.Rejected &&
                    x.wt.Status != WorkTaskStatus.Cancelled)
                || (bucket == "approved" &&
                    (x.wt.Status == WorkTaskStatus.Completed ||
                     x.wt.Status == WorkTaskStatus.Accepted ||
                     x.wt.Status == WorkTaskStatus.Approved))
                || (bucket == "rejected" &&
                    (x.wt.Status == WorkTaskStatus.Rejected ||
                     x.wt.Status == WorkTaskStatus.Cancelled)));
        }

        if (!string.IsNullOrWhiteSpace(input.CarPlate) ||
            !string.IsNullOrWhiteSpace(input.Vin) ||
            !string.IsNullOrWhiteSpace(input.EngineNumber))
        {
            var matchingClaimIds = await GetMatchingClaimIdsByVehicleNormalizedAsync(input.CarPlate, input.Vin, input.EngineNumber);
            if (matchingClaimIds.Count == 0)
            {
                return new PagedResultDto<QuotationApprovalListRowDto>(0, new List<QuotationApprovalListRowDto>());
            }

            filtered = filtered.Where(x => matchingClaimIds.Contains(x.c.Id));
        }

        var projected =
            from x in filtered
            join lob in lobQ on x.c.LobId equals lob.Id into lobG
            from lob in lobG.DefaultIfEmpty()
            join ins in insQ on x.c.InsurerId equals ins.Id into insG
            from ins in insG.DefaultIfEmpty()
            join prod in prodQ on x.cf.ProductId equals prod.Id into prodG
            from prod in prodG.DefaultIfEmpty()
            join rep in empQ on x.wt.ReporterId equals rep.Id into repG
            from rep in repG.DefaultIfEmpty()
            orderby (
                    (x.wt.Status == WorkTaskStatus.Rejected || x.wt.Status == WorkTaskStatus.Cancelled) ? 2 :
                    ((x.wt.Status == WorkTaskStatus.Completed || x.wt.Status == WorkTaskStatus.Accepted || x.wt.Status == WorkTaskStatus.Approved) ? 1 : 0)),
                x.wt.CreationTime descending
            let totalPasc = qaQ
                .Where(qa => qa.ClaimFolderId == x.cf.Id && !qa.IsDeleted)
                .OrderByDescending(qa => qa.CreationTime)
                .Select(qa => (decimal?)qa.ClaimAmount)
                .FirstOrDefault() ?? 0m
            select new QuotationApprovalListRowDto
            {
                WorkTaskId = x.wt.Id,
                ClaimId = x.c.Id,
                ClaimFolderId = x.cf.Id,
                ClaimCode = x.c.Code ?? string.Empty,
                FolderNo = x.cf.FolderNo ?? string.Empty,
                InsurerCode = ins.Code,
                LobName = lob.Name,
                ProductName = prod.Name,
                CarPlate = null,
                ReporterName = rep.FullName,
                CreationTime = x.wt.CreationTime,
                ActualEndDate = x.wt.ActualEndDate,
                IncidentDate = null,
                OnLocation = null,
                TotalPascAmount = totalPasc,
                // Same strings as WorkTaskConfiguration (enum.ToString().ToLowerInvariant); avoids CAST issues with varchar status column.
                WorkTaskStatus =
                    x.wt.Status == WorkTaskStatus.New ? "new" :
                    x.wt.Status == WorkTaskStatus.InProgress ? "inprogress" :
                    x.wt.Status == WorkTaskStatus.Completed ? "completed" :
                    x.wt.Status == WorkTaskStatus.Accepted ? "accepted" :
                    x.wt.Status == WorkTaskStatus.Rejected ? "rejected" :
                    x.wt.Status == WorkTaskStatus.Cancelled ? "cancelled" :
                    x.wt.Status == WorkTaskStatus.WaitApprove ? "waitapprove" :
                    x.wt.Status == WorkTaskStatus.Approved ? "approved" :
                    x.wt.Status == WorkTaskStatus.Pending ? "pending" :
                    x.wt.Status == WorkTaskStatus.Return ? "return" :
                    x.wt.Status == WorkTaskStatus.Transfer ? "transfer" : "new",
                ApprovalUiStatus = string.Empty,
                DetailAssessmentWorkTaskId = null,
            };

        var totalCount = await AsyncExecuter.CountAsync(projected);
        var take = input.MaxResultCount > 0 ? input.MaxResultCount : 10;
        var pagedRows = await AsyncExecuter.ToListAsync(projected.Skip(input.SkipCount).Take(take));

        await EnrichRowsWithIncidentAndMotorAsync(pagedRows);
        await EnrichRowsWithDetailAssessmentWorkTaskIdsAsync(pagedRows);
        return new PagedResultDto<QuotationApprovalListRowDto>(totalCount, pagedRows);
    }

    [Authorize(ClaimPermissions.QuotationApprovalList)]
    public virtual async Task ApproveQuotationApprovalAsync(ApproveQuotationApprovalInput input)
    {
        var currentEmployeeId = await GetCurrentEmployeeIdAsync();
        var wt = await GetRepairApprovalTaskForAssigneeAsync(input.WorkTaskId, currentEmployeeId);
        if (!IsRepairApprovalActionable(wt.Status))
        {
            throw new Volo.Abp.UserFriendlyException("Trạng thái công việc không cho phép phê duyệt.");
        }

        var qa = await QuotationApprovalRepository.FindAsync(wt.BusinessKey);
        if (qa == null || qa.Status != ClaimFolderQuotationApprovalStatus.Pending_Approval)
        {
            throw new Volo.Abp.UserFriendlyException("Không tìm thấy PASC chờ duyệt.");
        }

        qa.SetApproval(currentEmployeeId, Clock.Now);

        await QuotationApprovalRepository.UpdateAsync(qa, autoSave: true);

        if (qa.ClaimFolderId.HasValue)
        {
            await SyncClaimFolderItemPlansFromRepairDataAsync(qa.ClaimFolderId.Value, qa.PartnerId, qa.Data);
        }

        wt.UpdateStatus(WorkTaskStatus.Approved);
        wt.UpdateActualDates(wt.ActualStartDate ?? Clock.Now, Clock.Now);
        await WorkTaskRepository.UpdateAsync(wt, autoSave: true);

        await TriggerElsaForWorkTaskAsync(wt, "approve", null);
    }

    [Authorize(ClaimPermissions.QuotationApprovalList)]
    public virtual async Task RejectQuotationApprovalAsync(RejectQuotationApprovalInput input)
    {
        if (input.ReasonId == Guid.Empty)
        {
            throw new Volo.Abp.UserFriendlyException("Vui lòng chọn lý do từ chối.");
        }

        var currentEmployeeId = await GetCurrentEmployeeIdAsync();
        var wt = await GetRepairApprovalTaskForAssigneeAsync(input.WorkTaskId, currentEmployeeId);
        if (!IsRepairApprovalActionable(wt.Status))
        {
            throw new Volo.Abp.UserFriendlyException("Trạng thái công việc không cho phép từ chối.");
        }

        var qa = await QuotationApprovalRepository.FindAsync(wt.BusinessKey);
        if (qa == null || qa.Status != ClaimFolderQuotationApprovalStatus.Pending_Approval)
        {
            throw new Volo.Abp.UserFriendlyException("Không tìm thấy PASC chờ duyệt.");
        }

        qa.SetRejection(input.ReasonId, input.Comment?.Trim());

        await QuotationApprovalRepository.UpdateAsync(qa, autoSave: true);

        wt.UpdateRejectionReason(input.ReasonId, input.Comment?.Trim());
        wt.UpdateStatus(WorkTaskStatus.Rejected);
        wt.UpdateActualDates(wt.ActualStartDate, Clock.Now);
        await WorkTaskRepository.UpdateAsync(wt, autoSave: true);

        await TriggerElsaForWorkTaskAsync(wt, "reject", null);
    }

    [Authorize(ClaimPermissions.QuotationApprovalList)]
    public virtual async Task ReassignQuotationApprovalAsync(ReassignQuotationApprovalInput input)
    {
        if (input.ReasonId == Guid.Empty)
        {
            throw new Volo.Abp.UserFriendlyException("Vui lòng chọn lý do điều chuyển.");
        }

        if (string.IsNullOrWhiteSpace(input.ReasonDescription))
        {
            throw new Volo.Abp.UserFriendlyException("Vui lòng nhập mô tả lý do điều chuyển.");
        }

        if (input.NewAssigneeId == Guid.Empty)
        {
            throw new Volo.Abp.UserFriendlyException("Vui lòng chọn người duyệt mới.");
        }

        var currentEmployeeId = await GetCurrentEmployeeIdAsync();
        if (input.NewAssigneeId == currentEmployeeId)
        {
            throw new Volo.Abp.UserFriendlyException("Người được điều chuyển phải khác người đang duyệt.");
        }

        var wt = await GetRepairApprovalTaskForAssigneeAsync(input.WorkTaskId, currentEmployeeId);
        if (!IsRepairApprovalActionable(wt.Status))
        {
            throw new Volo.Abp.UserFriendlyException("Trạng thái công việc không cho phép điều chuyển.");
        }

        var qa = await QuotationApprovalRepository.FindAsync(wt.BusinessKey);
        if (qa == null || qa.Status != ClaimFolderQuotationApprovalStatus.Pending_Approval)
        {
            throw new Volo.Abp.UserFriendlyException("Không tìm thấy PASC chờ duyệt.");
        }

        var newEmpExists = await AsyncExecuter.AnyAsync(
            (await EmployeeRepository.GetQueryableAsync())
            .Where(e => e.Id == input.NewAssigneeId && !e.IsDeleted));

        if (!newEmpExists)
        {
            throw new Volo.Abp.UserFriendlyException("Người được nhận không hợp lệ.");
        }

        qa.UpdateApproverId(input.NewAssigneeId);
        var transferNote = !string.IsNullOrWhiteSpace(input.Comment)
            ? input.Comment.Trim()
            : input.ReasonDescription.Trim();
        if (!string.IsNullOrWhiteSpace(transferNote))
        {
            qa.UpdateDescription(transferNote);
        }

        await QuotationApprovalRepository.UpdateAsync(qa, autoSave: true);

        wt.UpdateRejectionReason(input.ReasonId, input.ReasonDescription.Trim());
        wt.UpdateActualDates(wt.ActualStartDate, Clock.Now);
        wt.UpdateStatus(WorkTaskStatus.Return);
        await WorkTaskRepository.UpdateAsync(wt, autoSave: true);

        await TriggerElsaForWorkTaskAsync(wt, "next", input.NewAssigneeId.ToString());
    }

    // ──────────────────────────────────────────────────────────
    // PRIVATE HELPERS
    // ──────────────────────────────────────────────────────────

    private const string PascNotEligibleMessage =
        "Phương án sửa chữa (PASC) chỉ áp dụng cho hồ sơ sản phẩm vật chất xe (VCX).";

    /// <summary>
    /// Giống logic folder trong <see cref="GetInitDataAsync"/> nhưng không ném lỗi (dùng cho tab eligibility).
    /// </summary>
    private async Task<Guid?> TryResolveRepairPlanInitFolderIdAsync(Guid claimId, Guid workTaskId)
    {
        var workTask = await AsyncExecuter.FirstOrDefaultAsync(
            (await WorkTaskRepository.GetQueryableAsync())
                .Where(x => x.Id == workTaskId)
                .Select(x => new { x.Id, x.BusinessCode, x.BusinessKey }));

        if (workTask == null)
        {
            return null;
        }

        if (workTask.BusinessCode == RepairPlanApprovalBusinessCode)
        {
            var quotationApproval = await QuotationApprovalRepository.FindAsync(workTask.BusinessKey);
            if (quotationApproval == null || quotationApproval.ClaimId != claimId)
            {
                return null;
            }

            if (quotationApproval.ClaimFolderId.HasValue)
            {
                return await AsyncExecuter.FirstOrDefaultAsync(
                    (await ClaimFolderRepository.GetQueryableAsync())
                        .Where(x => x.Id == quotationApproval.ClaimFolderId.Value && x.ClaimId == claimId)
                        .Select(x => (Guid?)x.Id));
            }

            return await AsyncExecuter.FirstOrDefaultAsync(
                (await ClaimFolderRepository.GetQueryableAsync())
                    .Where(x => x.ClaimId == claimId)
                    .OrderByDescending(x => x.CreationTime)
                    .Select(x => (Guid?)x.Id));
        }

        if (workTask.BusinessCode == DetailAssessmentBusinessCode)
        {
            try
            {
                var claimFolderId = await ResolveClaimFolderIdFromAssessmentWorkTaskAsync(claimId, workTaskId);

                return await AsyncExecuter.FirstOrDefaultAsync(
                    (await ClaimFolderRepository.GetQueryableAsync())
                        .Where(x => x.Id == claimFolderId && x.ClaimId == claimId)
                        .Select(x => (Guid?)x.Id));
            }
            catch
            {
                return null;
            }
        }

        var claimFolderKey = workTask.BusinessKey;
        return await AsyncExecuter.FirstOrDefaultAsync(
            (await ClaimFolderRepository.GetQueryableAsync())
                .Where(x => x.Id == claimFolderKey || x.ClaimId == claimId)
                .OrderByDescending(x => x.CreationTime)
                .Select(x => (Guid?)x.Id));
    }

    private async Task<bool> IsPascEligibleForFolderAsync(Guid claimFolderId)
    {
        var productId = await AsyncExecuter.FirstOrDefaultAsync(
            (await ClaimFolderRepository.GetQueryableAsync())
                .Where(f => f.Id == claimFolderId)
                .Select(f => f.ProductId));

        if (!productId.HasValue)
        {
            return false;
        }

        var productTypeId = await AsyncExecuter.FirstOrDefaultAsync(
            (await ProProductRepository.GetQueryableAsync())
                .Where(p => p.Id == productId.Value && !p.IsDeleted)
                .Select(p => p.ProductTypeId));

        if (!productTypeId.HasValue)
        {
            return false;
        }

        var typeCode = await AsyncExecuter.FirstOrDefaultAsync(
            (await ProProductTypeRepository.GetQueryableAsync())
                .Where(t => t.Id == productTypeId.Value && !t.IsDeleted)
                .Select(t => t.Code));

        return string.Equals(typeCode, PascEligibleProductTypeCode, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Xác định ClaimFolderId khi lưu PASC: ưu tiên WorkTaskId (giám định chi tiết), sau đó ClaimFolderId nếu hợp lệ.
    /// </summary>
    private async Task<Guid?> ResolveClaimFolderIdForSaveAsync(SaveRepairPlanInput input)
    {
        if (input.WorkTaskId.HasValue && input.WorkTaskId.Value != Guid.Empty)
        {
            return await ResolveClaimFolderIdFromAssessmentWorkTaskAsync(input.ClaimId, input.WorkTaskId.Value);
        }

        if (input.ClaimFolderId.HasValue && input.ClaimFolderId.Value != Guid.Empty)
        {
            var matches = await AsyncExecuter.AnyAsync(
                (await ClaimFolderRepository.GetQueryableAsync())
                    .Where(x => x.Id == input.ClaimFolderId.Value && x.ClaimId == input.ClaimId));
            if (!matches)
            {
                throw new Volo.Abp.UserFriendlyException("Hồ sơ bồi thường không khớp với yêu cầu bồi thường.");
            }

            return input.ClaimFolderId.Value;
        }

        return null;
    }

    private async Task<Guid> ResolveClaimFolderIdFromAssessmentWorkTaskAsync(Guid claimId, Guid workTaskId)
    {
        var workTask = await AsyncExecuter.FirstOrDefaultAsync(
            (await WorkTaskRepository.GetQueryableAsync())
            .Where(x => x.Id == workTaskId)
            .Select(x => new { x.BusinessCode, x.BusinessKey }));

        if (workTask == null)
        {
            throw new Volo.Abp.UserFriendlyException("Không tìm thấy công việc.");
        }

        // Luồng phê duyệt PASC: BusinessKey = ClaimFolderQuotationApproval.Id (sau SubmitAsync / Elsa), không phải ClaimFolder.Id.
        if (workTask.BusinessCode == RepairPlanApprovalBusinessCode)
        {
            var quotationApproval = await QuotationApprovalRepository.FindAsync(workTask.BusinessKey);
            if (quotationApproval == null)
            {
                throw new Volo.Abp.UserFriendlyException("Không tìm thấy hồ sơ phê duyệt PASC.");
            }

            if (quotationApproval.ClaimId != claimId)
            {
                throw new Volo.Abp.UserFriendlyException("Không khớp yêu cầu bồi thường.");
            }

            if (quotationApproval.ClaimFolderId.HasValue)
            {
                var matchesApprovalFolder = await AsyncExecuter.AnyAsync(
                    (await ClaimFolderRepository.GetQueryableAsync())
                        .Where(x => x.Id == quotationApproval.ClaimFolderId.Value && x.ClaimId == claimId));

                if (!matchesApprovalFolder)
                {
                    throw new Volo.Abp.UserFriendlyException("Hồ sơ bồi thường không khớp với yêu cầu bồi thường.");
                }

                return quotationApproval.ClaimFolderId.Value;
            }

            var latestFolderId = await AsyncExecuter.FirstOrDefaultAsync(
                (await ClaimFolderRepository.GetQueryableAsync())
                    .Where(x => x.ClaimId == claimId)
                    .OrderByDescending(x => x.CreationTime)
                    .Select(x => (Guid?)x.Id));

            if (!latestFolderId.HasValue)
            {
                throw new Volo.Abp.UserFriendlyException("Không tìm thấy hồ sơ bồi thường.");
            }

            return latestFolderId.Value;
        }

        var claimFolderId = workTask.BusinessKey;

        var matches = await AsyncExecuter.AnyAsync(
            (await ClaimFolderRepository.GetQueryableAsync())
                .Where(x => x.Id == claimFolderId && x.ClaimId == claimId));

        if (!matches)
        {
            throw new Volo.Abp.UserFriendlyException("Hồ sơ bồi thường không khớp với yêu cầu bồi thường.");
        }

        return claimFolderId;
    }

    private async Task SyncRepairPlanApprovalWorkTaskAsync(Guid quotationApprovalId, Guid approverEmployeeId, string? description)
    {
        var approverEmp = await AsyncExecuter.FirstOrDefaultAsync(
            (await EmployeeRepository.GetQueryableAsync())
            .Where(x => x.Id == approverEmployeeId && !x.IsDeleted)
            .Select(x => new { x.Id, x.DepartmentId, x.OrgId }));

        if (approverEmp == null)
        {
            throw new Volo.Abp.UserFriendlyException("Không tìm thấy nhân viên được chọn.");
        }

        await ElsaWorkflowService.InitCreateRepairPlanWorkflowAsync(quotationApprovalId, approverEmployeeId.ToString());
    }

    private async Task<List<RepairPlanIncidentObjectDto>> BuildIncidentObjectsAsync(Guid claimFolderId)
    {
        var incidentObjectQuery = await IncidentObjectRepository.GetQueryableAsync();
        var objectTypeQuery = await ResObjectTypeRepository.GetQueryableAsync();

        var result = await AsyncExecuter.ToListAsync(
            incidentObjectQuery
            .Where(x => x.ClaimFolderId == claimFolderId)
            .Join(objectTypeQuery, io => io.ObjectTypeId, ot => ot.Id,
                (io, ot) => new RepairPlanIncidentObjectDto
                {
                    Id = io.Id,
                    ObjectTypeId = io.ObjectTypeId,
                    ObjectTypeName = ot.Name,
                    CarPlate = io.CarPlate,
                }));

        return result;
    }

    private async Task<List<RepairPlanAssessmentItemDto>> BuildAssessmentItemsAsync(Guid claimFolderId)
    {
        var itemQuery = await ClaimFolderItemRepository.GetQueryableAsync();
        var objectTypeItemQuery = await ResObjectTypeItemRepository.GetQueryableAsync();
        var claimPlanQuery = await ResClaimPlanRepository.GetQueryableAsync();

        // ClaimFolderItem đã lưu sẵn DepreciationPercent được tính từ bước giám định chi tiết.
        // Bỏ qua dòng không có ItemId (ví dụ section con người) vì repair plan đang yêu cầu hạng mục xe cụ thể.
        var rawItems = await AsyncExecuter.ToListAsync(
            itemQuery
            .Where(x => x.ClaimFolderId == claimFolderId && x.ItemId.HasValue)
            .OrderBy(x => x.CreationTime)
            .Select(x => new
            {
                x.Id,
                ItemId = x.ItemId!.Value,
                x.ClaimPlanId,      // Guid?
                x.Quantity,
                x.DepreciationPercent,
            }));

        if (rawItems.Count == 0)
        {
            return new List<RepairPlanAssessmentItemDto>();
        }

        var itemIds = rawItems.Select(x => x.ItemId).Distinct().ToList();
        var planIds = rawItems.Where(x => x.ClaimPlanId.HasValue).Select(x => x.ClaimPlanId!.Value).Distinct().ToList();

        var objectTypeItems = await AsyncExecuter.ToListAsync(
            objectTypeItemQuery
            .Where(x => itemIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Name }));

        var claimPlans = await AsyncExecuter.ToListAsync(
            claimPlanQuery
            .Where(x => planIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Name }));

        var itemNameMap = objectTypeItems.ToDictionary(x => x.Id, x => x.Name);
        var planNameMap = claimPlans.ToDictionary(x => x.Id, x => x.Name);

        var result = new List<RepairPlanAssessmentItemDto>();
        foreach (var item in rawItems)
        {
            var itemName = itemNameMap.TryGetValue(item.ItemId, out var n) ? n : "(Không xác định)";
            var planName = item.ClaimPlanId.HasValue && planNameMap.TryGetValue(item.ClaimPlanId.Value, out var p)
                ? p
                : string.Empty;

            result.Add(new RepairPlanAssessmentItemDto
            {
                Id = item.Id,
                ItemId = item.ItemId,
                ItemName = itemName,
                PlanName = planName,
                ClaimPlanId = item.ClaimPlanId,
                Quantity = item.Quantity,
                ObjectTypeItemId = item.ItemId,
                DepreciationPercent = item.DepreciationPercent,
            });
        }

        return result;
    }

    /// <summary>
    /// Trạng thái PASC cho init: displayable trước (New…Done), không có thì nháp (New / Pending_Approval).
    /// </summary>
    private async Task<string?> ResolveInitQuotationApprovalStatusForFolderAsync(Guid claimId, Guid? claimFolderId)
    {
        if (!claimFolderId.HasValue || claimFolderId.Value == Guid.Empty)
        {
            return null;
        }

        var displayable = await FindLatestDisplayableQuotationApprovalAsync(claimId, claimFolderId.Value);
        if (displayable != null)
        {
            return ClaimFolderQuotationApprovalStatusDatabase.ToColumnValue(displayable.Status);
        }

        var draft = await FindExistingDraftAsync(claimId, claimFolderId);
        return draft != null ? ClaimFolderQuotationApprovalStatusDatabase.ToColumnValue(draft.Status) : null;
    }

    /// <summary>
    /// Tìm bản nháp/chờ duyệt theo claim + hồ sơ (ClaimFolderId). Nếu có folder nhưng chỉ tồn tại bản cũ
    /// (ClaimFolderId null), trả về bản đó để cập nhật và gán folder — sửa lỗi F5 sau khi lưu.
    /// </summary>
    private async Task<ClaimFolderQuotationApproval?> FindExistingDraftAsync(Guid claimId, Guid? claimFolderId)
    {
        var baseQuery =
            (await QuotationApprovalRepository.GetQueryableAsync())
                .Where(x => x.ClaimId == claimId &&
                    (x.Status == ClaimFolderQuotationApprovalStatus.New ||
                     x.Status == ClaimFolderQuotationApprovalStatus.Pending_Approval));

        if (claimFolderId.HasValue && claimFolderId.Value != Guid.Empty)
        {
            var exact = await AsyncExecuter.FirstOrDefaultAsync(
                baseQuery
                    .Where(x => x.ClaimFolderId == claimFolderId.Value)
                    .OrderByDescending(x => x.CreationTime));
            if (exact != null)
            {
                return exact;
            }

            // Legacy: draft đã lưu trước khi gán ClaimFolderId
            return await AsyncExecuter.FirstOrDefaultAsync(
                baseQuery
                    .Where(x => x.ClaimFolderId == null)
                    .OrderByDescending(x => x.CreationTime));
        }

        return await AsyncExecuter.FirstOrDefaultAsync(
            baseQuery
                .Where(x => x.ClaimFolderId == null)
                .OrderByDescending(x => x.CreationTime));
    }

    /// <summary>
    /// Bản ghi báo giá hiển thị trên màn chi tiết (gồm đã duyệt/hoàn thành), không dùng cho logic chỉ nháp.
    /// </summary>
    private async Task<ClaimFolderQuotationApproval?> FindLatestDisplayableQuotationApprovalAsync(
        Guid claimId,
        Guid claimFolderId)
    {
        return await AsyncExecuter.FirstOrDefaultAsync(
            (await QuotationApprovalRepository.GetQueryableAsync())
                .Where(x =>
                    x.ClaimId == claimId &&
                    x.ClaimFolderId == claimFolderId &&
                    (x.Status == ClaimFolderQuotationApprovalStatus.New ||
                     x.Status == ClaimFolderQuotationApprovalStatus.InProgress ||
                     x.Status == ClaimFolderQuotationApprovalStatus.Pending_Approval ||
                     x.Status == ClaimFolderQuotationApprovalStatus.Approved ||
                     x.Status == ClaimFolderQuotationApprovalStatus.Done))
                .OrderByDescending(x => x.CreationTime));
    }

    /// <summary>
    /// Chặn trình duyệt lặp khi đã có PASC/báo giá đang chờ duyệt, đã duyệt hoặc hoàn thành.
    /// </summary>
    private async Task EnsureCanSubmitQuotationApprovalAsync(Guid claimId, Guid claimFolderId)
    {
        var alreadyInApprovalFlow = await AsyncExecuter.AnyAsync(
            (await QuotationApprovalRepository.GetQueryableAsync())
                .Where(x =>
                    x.ClaimId == claimId &&
                    x.ClaimFolderId == claimFolderId &&
                    (x.Status == ClaimFolderQuotationApprovalStatus.Pending_Approval ||
                     x.Status == ClaimFolderQuotationApprovalStatus.Approved ||
                     x.Status == ClaimFolderQuotationApprovalStatus.Done)));

        if (alreadyInApprovalFlow)
        {
            throw new Volo.Abp.UserFriendlyException(
                "Hồ sơ đã được trình duyệt hoặc đang chờ duyệt, không thể trình duyệt lại.");
        }
    }

    private async Task<Guid> GetCurrentEmployeeIdAsync()
    {
        if (CurrentUser.Id == null)
        {
            throw new Volo.Abp.UserFriendlyException(L["UserNotAuthenticated"].Value);
        }

        var employee = await AsyncExecuter.FirstOrDefaultAsync(
            (await EmployeeRepository.GetQueryableAsync())
            .Where(x => x.UserId == CurrentUser.Id)
            .Select(x => new { x.Id }));

        if (employee == null)
        {
            throw new Volo.Abp.UserFriendlyException(L["ClaimTask:NotAuthorized"].Value);
        }

        return employee.Id;
    }

    private async Task TriggerElsaForWorkTaskAsync(WorkTask workTask, string action, string? assigneeId)
    {
        if (workTask.WorkInstanceId == null)
        {
            return;
        }

        var workInstance = await WorkInstanceRepository.GetAsync(workTask.WorkInstanceId.Value);
        var eventName = workTask.EventName ?? workInstance.WorkflowInstanceId;
        await ElsaWorkflowService.TriggerApprovalAsync(eventName, workInstance.WorkflowInstanceId, action, assigneeId);
    }

    private async Task<WorkTask> GetRepairApprovalTaskForAssigneeAsync(Guid workTaskId, Guid employeeId)
    {
        var wt = await WorkTaskRepository.GetAsync(workTaskId);
        if (wt.BusinessCode != RepairPlanApprovalBusinessCode || wt.AssigneeId != employeeId)
        {
            throw new Volo.Abp.UserFriendlyException(L["ClaimTask:NotAuthorized"].Value);
        }

        return wt;
    }

    private static bool IsRepairApprovalActionable(WorkTaskStatus status)
    {
        return status is WorkTaskStatus.New
            or WorkTaskStatus.WaitApprove
            or WorkTaskStatus.Pending
            or WorkTaskStatus.InProgress;
    }

    private async Task EnrichRowsWithDetailAssessmentWorkTaskIdsAsync(List<QuotationApprovalListRowDto> rows)
    {
        if (rows.Count == 0)
        {
            return;
        }

        var folderIds = rows.Select(r => r.ClaimFolderId).Distinct().ToList();
        var wtQ = await WorkTaskRepository.GetQueryableAsync();
        var candidates = await AsyncExecuter.ToListAsync(
            wtQ.Where(wt =>
                    folderIds.Contains(wt.BusinessKey)
                    && wt.BusinessCode == DetailAssessmentBusinessCode)
                .Select(wt => new { wt.Id, wt.BusinessKey, wt.CreationTime }));

        var latestByFolder = candidates
            .GroupBy(x => x.BusinessKey)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.CreationTime).First().Id);

        foreach (var row in rows)
        {
            if (latestByFolder.TryGetValue(row.ClaimFolderId, out var wtId))
            {
                row.DetailAssessmentWorkTaskId = wtId;
            }
        }
    }

    private async Task EnrichRowsWithIncidentAndMotorAsync(List<QuotationApprovalListRowDto> rows)
    {
        if (rows.Count == 0)
        {
            return;
        }

        foreach (var r in rows)
        {
            r.ApprovalUiStatus = MapUiApprovalStatus(r.WorkTaskStatus);
        }

        var claimIds = rows.Select(r => r.ClaimId).Distinct().ToList();
        var incidents = await AsyncExecuter.ToListAsync(
            (await ClaimIncidentRepository.GetQueryableAsync())
                .Where(ci => claimIds.Contains(ci.IncidentId)));

        var incidentByClaimId = new Dictionary<Guid, ClaimIncident>();
        foreach (var g in incidents.GroupBy(i => i.IncidentId))
        {
            var first = g.OrderByDescending(i => i.CreationTime).First();
            incidentByClaimId[g.Key] = first;
        }

        var incidentDbIds = incidentByClaimId.Values.Select(i => i.Id).Distinct().ToList();
        var motorsByIncidentId = new Dictionary<Guid, ClaimIncidentRiskMotor>();
        if (incidentDbIds.Count > 0)
        {
            var motors = await AsyncExecuter.ToListAsync(
                (await ClaimIncidentRiskMotorRepository.GetQueryableAsync())
                    .Where(rm => rm.IncidentObjectId.HasValue && incidentDbIds.Contains(rm.IncidentObjectId.Value)));
            motorsByIncidentId = motors.GroupBy(m => m.IncidentObjectId!.Value).ToDictionary(g => g.Key, g => g.First());
        }

        foreach (var row in rows)
        {
            if (!incidentByClaimId.TryGetValue(row.ClaimId, out var inc))
            {
                continue;
            }

            row.IncidentDate = inc.IncidentDate;
            row.OnLocation = inc.OnLocation;
            if (motorsByIncidentId.TryGetValue(inc.Id, out var motor))
            {
                row.CarPlate = motor.CarPlate;
            }
        }
    }

    private async Task<HashSet<Guid>> GetMatchingClaimIdsByVehicleNormalizedAsync(string? carPlate, string? vin, string? engineNumber)
    {
        var claimIncidentQuery = await ClaimIncidentRepository.GetQueryableAsync();
        var riskMotorQuery = await ClaimIncidentRiskMotorRepository.GetQueryableAsync();
        var pairs = await AsyncExecuter.ToListAsync(
            from ci in claimIncidentQuery
            join rm in riskMotorQuery on ci.Id equals rm.IncidentObjectId into riskMotors
            from rm in riskMotors.DefaultIfEmpty()
            select new { ClaimId = ci.IncidentId, RiskMotor = rm });

        var result = new HashSet<Guid>();
        foreach (var item in pairs)
        {
            var match = true;
            if (!string.IsNullOrWhiteSpace(carPlate))
            {
                var normalizedInput = NormalizeRepairVehicle(carPlate);
                var normalizedValue = item.RiskMotor != null && !string.IsNullOrEmpty(item.RiskMotor.CarPlate)
                    ? NormalizeRepairVehicle(item.RiskMotor.CarPlate)
                    : string.Empty;
                if (!normalizedValue.Contains(normalizedInput))
                {
                    match = false;
                }
            }

            if (match && !string.IsNullOrWhiteSpace(vin))
            {
                var normalizedInput = NormalizeRepairVehicle(vin);
                var normalizedValue = item.RiskMotor != null && !string.IsNullOrEmpty(item.RiskMotor.Vin)
                    ? NormalizeRepairVehicle(item.RiskMotor.Vin)
                    : string.Empty;
                if (normalizedValue != normalizedInput)
                {
                    match = false;
                }
            }

            if (match && !string.IsNullOrWhiteSpace(engineNumber))
            {
                var normalizedInput = NormalizeRepairVehicle(engineNumber);
                var normalizedValue = item.RiskMotor != null && !string.IsNullOrEmpty(item.RiskMotor.EngineNumber)
                    ? NormalizeRepairVehicle(item.RiskMotor.EngineNumber)
                    : string.Empty;
                if (normalizedValue != normalizedInput)
                {
                    match = false;
                }
            }

            if (match)
            {
                result.Add(item.ClaimId);
            }
        }

        return result;
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

    private static string NormalizeRepairVehicle(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return Regex.Replace(value, @"[^A-Za-z0-9]", "").ToUpperInvariant();
    }

    /// <summary>Maps work-task status string to list UI bucket. Return/Transfer rows are excluded from <see cref="GetApprovalListAsync"/>.</summary>
    private static string MapUiApprovalStatus(string workTaskStatus)
    {
        if (!Enum.TryParse<WorkTaskStatus>(workTaskStatus, ignoreCase: true, out var s))
        {
            return "new";
        }

        if (s == WorkTaskStatus.Return || s == WorkTaskStatus.Transfer)
        {
            return "new";
        }

        if (s == WorkTaskStatus.Rejected || s == WorkTaskStatus.Cancelled)
        {
            return "rejected";
        }

        if (s == WorkTaskStatus.Completed || s == WorkTaskStatus.Accepted || s == WorkTaskStatus.Approved)
        {
            return "approved";
        }

        return "new";
    }

    private async Task SyncClaimFolderItemPlansFromRepairDataAsync(
        Guid? claimFolderId,
        Guid? partnerId,
        string? dataJson)
    {
        if (!claimFolderId.HasValue || claimFolderId.Value == Guid.Empty)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(dataJson))
        {
            return;
        }

        RepairPlanFormJsonRoot? form;
        try
        {
            form = JsonSerializer.Deserialize<RepairPlanFormJsonRoot>(dataJson, RepairPlanJsonOptions);
        }
        catch (JsonException)
        {
            return;
        }

        if (form?.Items == null || form.Items.Count == 0)
        {
            return;
        }

        var folderItemQuery = await ClaimFolderItemRepository.GetQueryableAsync();
        var folderItems = await AsyncExecuter.ToListAsync(
            folderItemQuery.Where(x => x.ClaimFolderId == claimFolderId.Value));
        var folderItemById = folderItems.ToDictionary(x => x.Id, x => x);

        var planKeysToRefresh = new HashSet<(Guid ItemId, Guid PlanId)>();
        foreach (var item in form.Items)
        {
            if (!Guid.TryParse(item.Id, out var claimFolderItemId))
            {
                continue;
            }

            if (!folderItemById.TryGetValue(claimFolderItemId, out var cfItem))
            {
                continue;
            }

            var planId = cfItem.ClaimPlanId;
            if (!planId.HasValue && Guid.TryParse(item.ClaimPlanId, out var jsonPlanId))
            {
                planId = jsonPlanId;
            }

            if (!planId.HasValue)
            {
                continue;
            }

            planKeysToRefresh.Add((claimFolderItemId, planId.Value));
        }

        if (planKeysToRefresh.Count == 0)
        {
            return;
        }

        var itemIds = planKeysToRefresh.Select(k => k.ItemId).Distinct().ToList();
        var itemPlanQuery = await ClaimFolderItemPlanRepository.GetQueryableAsync();
        var existingPlans = await AsyncExecuter.ToListAsync(
            itemPlanQuery.Where(p => itemIds.Contains(p.ClaimFolderItemId)));

        foreach (var plan in existingPlans)
        {
            if (planKeysToRefresh.Contains((plan.ClaimFolderItemId, plan.ClaimPlanId)))
            {
                await ClaimFolderItemPlanRepository.DeleteAsync(plan);
            }
        }

        foreach (var item in form.Items)
        {
            if (!Guid.TryParse(item.Id, out var claimFolderItemId))
            {
                continue;
            }

            if (!folderItemById.TryGetValue(claimFolderItemId, out var cfItem))
            {
                continue;
            }

            var planId = cfItem.ClaimPlanId;
            if (!planId.HasValue && Guid.TryParse(item.ClaimPlanId, out var jsonPlanId))
            {
                planId = jsonPlanId;
            }

            if (!planId.HasValue)
            {
                continue;
            }

            foreach (var row in item.CostRows ?? new List<RepairPlanFormJsonCostRow>())
            {
                var planType = (row.CostType ?? string.Empty).Trim().ToLowerInvariant();
                if (!RepairPlanCostTypes.Contains(planType))
                {
                    continue;
                }

                var totals = ComputeRepairLineAmounts(row, planType);
                var entity = new ClaimFolderItemPlan(
                    GuidGenerator.Create(),
                    claimFolderItemId,
                    planId.Value,
                    planType,
                    partnerId,
                    row.GarageAmount,
                    row.ProposedAmount,
                    totals.DiscountPercent,
                    totals.DiscountAmount,
                    totals.TotalAmount,
                    totals.DepreciationPercent,
                    totals.DepreciationAmount,
                    totals.ApprovedAmount);

                await ClaimFolderItemPlanRepository.InsertAsync(entity, autoSave: true);
            }
        }
    }

    private static RepairLineComputedAmounts ComputeRepairLineAmounts(RepairPlanFormJsonCostRow row, string planType)
    {
        var proposed = row.ProposedAmount ?? 0m;
        if (proposed < 0)
        {
            proposed = 0;
        }

        var discountAmt = row.DiscountAmount ?? 0m;
        if (discountAmt < 0)
        {
            discountAmt = 0;
        }

        var total = RoundMoney(decimal.Max(0, proposed - discountAmt));

        decimal? depPct = null;
        var depAmount = 0m;
        if (string.Equals(planType, "material_cost", StringComparison.OrdinalIgnoreCase))
        {
            var rawPct = row.DepreciationPercent ?? 0m;
            if (rawPct > 0)
            {
                depPct = rawPct;
                depAmount = RoundMoney(total * rawPct / 100m);
            }
        }

        var approved = RoundMoney(total - depAmount);

        decimal? discPct = row.DiscountPercent;
        if (discPct.HasValue && discPct.Value <= 0)
        {
            discPct = null;
        }

        return new RepairLineComputedAmounts(
            total,
            depPct,
            depAmount,
            approved,
            discPct,
            discountAmt > 0 ? discountAmt : null);
    }

    private static decimal RoundMoney(decimal v) =>
        decimal.Round(v, 0, MidpointRounding.AwayFromZero);

    private readonly record struct RepairLineComputedAmounts(
        decimal TotalAmount,
        decimal? DepreciationPercent,
        decimal DepreciationAmount,
        decimal ApprovedAmount,
        decimal? DiscountPercent,
        decimal? DiscountAmount);

    private static void ValidateSaveInput(SaveRepairPlanInput input)
    {
        if (input.ClaimId == Guid.Empty)
        {
            throw new Volo.Abp.UserFriendlyException("ClaimId là bắt buộc.");
        }

        if (input.ClaimAmount < 0)
        {
            throw new Volo.Abp.UserFriendlyException("Tổng tiền đề xuất không được âm.");
        }

        if (input.DiscountAmount < 0)
        {
            throw new Volo.Abp.UserFriendlyException("Tổng tiền giảm giá không được âm.");
        }

        if (input.DepreciationAmount < 0)
        {
            throw new Volo.Abp.UserFriendlyException("Tổng khấu hao không được âm.");
        }

        if (input.ExpenseAmount < 0)
        {
            throw new Volo.Abp.UserFriendlyException("Tổng chi phí không được âm.");
        }
    }
}
