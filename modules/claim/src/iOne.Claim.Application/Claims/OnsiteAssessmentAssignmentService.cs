using System;
using System.Linq;
using System.Threading.Tasks;
using iOne.Claim.Localization;
using iOne.ClaimAdjustAtLocations;
using iOne.ClaimIncidents;
using iOne.Claims;
using iOne.HrEmployees;
using iOne.Workflow;
using iOne.WorkTasks;
using Volo.Abp.Application.Services;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using ClaimEntity = iOne.Claims.Claim;

namespace iOne.Claim.Claims;

public interface IOnsiteAssessmentAssignmentService : ITransientDependency
{
    Task<ClaimAdjustAtLocation> CreateAsync(
        Guid claimId,
        Guid assigneeOrganizationId,
        Guid assigneeId,
        DateTime startDate,
        DateTime endDate);

    Task<bool> HasActiveOnsiteAssessmentAsync(Guid claimId);
}

public class OnsiteAssessmentAssignmentService : ApplicationService, IOnsiteAssessmentAssignmentService
{
    private readonly IRepository<ClaimEntity, Guid> _claimRepository;
    private readonly IRepository<ClaimAdjustAtLocation, Guid> _claimAdjustAtLocationRepository;
    private readonly IRepository<ClaimIncident, Guid> _claimIncidentRepository;
    private readonly IRepository<WorkTask, Guid> _workTaskRepository;
    private readonly IRepository<HrEmployee, Guid> _employeeRepository;
    private readonly IElsaWorkflowService _elsaWorkflowService;

    public OnsiteAssessmentAssignmentService(
        IRepository<ClaimEntity, Guid> claimRepository,
        IRepository<ClaimAdjustAtLocation, Guid> claimAdjustAtLocationRepository,
        IRepository<ClaimIncident, Guid> claimIncidentRepository,
        IRepository<WorkTask, Guid> workTaskRepository,
        IRepository<HrEmployee, Guid> employeeRepository,
        IElsaWorkflowService elsaWorkflowService)
    {
        _claimRepository = claimRepository;
        _claimAdjustAtLocationRepository = claimAdjustAtLocationRepository;
        _claimIncidentRepository = claimIncidentRepository;
        _workTaskRepository = workTaskRepository;
        _employeeRepository = employeeRepository;
        _elsaWorkflowService = elsaWorkflowService;
        LocalizationResource = typeof(ClaimResource);
    }

    public virtual async Task<ClaimAdjustAtLocation> CreateAsync(
        Guid claimId,
        Guid assigneeOrganizationId,
        Guid assigneeId,
        DateTime startDate,
        DateTime endDate)
    {
        var claim = await _claimRepository.GetAsync(claimId);
        if (claim.Status != ClaimStatus.InProgress)
        {
            throw new Volo.Abp.UserFriendlyException("Yêu cầu bồi thường không ở trạng thái đang xử lý.");
        }

        var incident = await _claimIncidentRepository.FirstOrDefaultAsync(x => x.IncidentId == claimId);
        if (incident?.OnLocation != "Y")
        {
            throw new Volo.Abp.UserFriendlyException("Yêu cầu bồi thường không có thông tin xe đang ở hiện trường.");
        }

        if (endDate < startDate)
        {
            throw new Volo.Abp.UserFriendlyException("Ngày dự kiến hoàn thành phải lớn hơn hoặc bằng ngày giám định.");
        }

        var assignee = await _employeeRepository.FirstOrDefaultAsync(x => x.Id == assigneeId);
        if (assignee == null)
        {
            throw new Volo.Abp.UserFriendlyException("Người giám định không hợp lệ.");
        }
        if (assignee.DepartmentId != assigneeOrganizationId)
        {
            throw new Volo.Abp.UserFriendlyException("Người giám định không thuộc đơn vị đã chọn.");
        }

        if (await HasActiveOnsiteAssessmentAsync(claimId))
        {
            throw new Volo.Abp.UserFriendlyException(L["ClaimTask:OnsiteAssessmentAlreadyProcessed"].Value);
        }

        var latestBeforeCreate = await GetLatestClaimAdjustAtLocationAsync(claimId);
        if (latestBeforeCreate?.Status == ClaimAdjustAtLocationStatus.Done)
        {
            throw new Volo.Abp.UserFriendlyException(L["ClaimTask:OnsiteAssessmentLatestRoundFinished"].Value);
        }

        var adjust = new ClaimAdjustAtLocation(
            GuidGenerator.Create(),
            claimId,
            assigneeId,
            startDate: startDate,
            endDate: endDate,
            status: ClaimAdjustAtLocationStatus.New);
        await _claimAdjustAtLocationRepository.InsertAsync(adjust, autoSave: true);

        await _elsaWorkflowService.InitOnsiteAssessmentWorkflowAsync(
            adjust.Id,
            assigneeOrganizationId,
            assigneeId,
            startDate,
            endDate);

        return adjust;
    }

    public virtual async Task<bool> HasActiveOnsiteAssessmentAsync(Guid claimId)
    {
        var latestAdjust = await GetLatestClaimAdjustAtLocationAsync(claimId);
        if (latestAdjust == null)
        {
            return false;
        }

        var onsiteAssessmentQuery = await _workTaskRepository.GetQueryableAsync();
        return await AsyncExecuter.AnyAsync(
            onsiteAssessmentQuery.Where(wt =>
                wt.BusinessCode == "CLAIM_ONSITE_ASSESSMENT"
                && wt.BusinessKey == latestAdjust.Id
                && wt.Status != WorkTaskStatus.Rejected
                && wt.Status != WorkTaskStatus.Cancelled
                && wt.Status != WorkTaskStatus.Completed
                && wt.Status != WorkTaskStatus.Return));
    }

    private async Task<ClaimAdjustAtLocation?> GetLatestClaimAdjustAtLocationAsync(Guid claimId)
    {
        var query = await _claimAdjustAtLocationRepository.GetQueryableAsync();
        return await AsyncExecuter.FirstOrDefaultAsync(
            query
                .Where(a => a.ClaimId == claimId)
                .OrderByDescending(a => a.CreationTime)
                .ThenByDescending(a => a.Id));
    }
}
