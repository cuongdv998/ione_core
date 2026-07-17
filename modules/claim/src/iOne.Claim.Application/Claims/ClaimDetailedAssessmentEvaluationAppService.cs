using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using iOne.Claim.Localization;
using iOne.ClaimFolderEvaluateDetails;
using iOne.ClaimFolderEvaluates;
using iOne.ClaimFolders;
using iOne.HrEmployees;
using iOne.ResClaimEvaluateItems;
using iOne.WorkTasks;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace iOne.Claim.Claims;

public class ClaimDetailedAssessmentEvaluationAppService : ApplicationService, IClaimDetailedAssessmentEvaluationAppService
{
    protected IRepository<WorkTask, Guid> WorkTaskRepository { get; }
    protected IRepository<ClaimFolder, Guid> ClaimFolderRepository { get; }
    protected IRepository<HrEmployee, Guid> EmployeeRepository { get; }
    protected IRepository<ResClaimEvaluateItem, Guid> ResClaimEvaluateItemRepository { get; }
    protected IRepository<ClaimFolderEvaluate, Guid> ClaimFolderEvaluateRepository { get; }
    protected IRepository<ClaimFolderEvaluateDetail, Guid> ClaimFolderEvaluateDetailRepository { get; }

    public ClaimDetailedAssessmentEvaluationAppService(
        IRepository<WorkTask, Guid> workTaskRepository,
        IRepository<ClaimFolder, Guid> claimFolderRepository,
        IRepository<HrEmployee, Guid> employeeRepository,
        IRepository<ResClaimEvaluateItem, Guid> resClaimEvaluateItemRepository,
        IRepository<ClaimFolderEvaluate, Guid> claimFolderEvaluateRepository,
        IRepository<ClaimFolderEvaluateDetail, Guid> claimFolderEvaluateDetailRepository)
    {
        WorkTaskRepository = workTaskRepository;
        ClaimFolderRepository = claimFolderRepository;
        EmployeeRepository = employeeRepository;
        ResClaimEvaluateItemRepository = resClaimEvaluateItemRepository;
        ClaimFolderEvaluateRepository = claimFolderEvaluateRepository;
        ClaimFolderEvaluateDetailRepository = claimFolderEvaluateDetailRepository;
        LocalizationResource = typeof(ClaimResource);
    }

    public virtual async Task<DetailedAssessmentEvaluationDetailDto> GetDetailAsync(Guid workTaskId)
    {
        var currentEmployeeId = await GetCurrentEmployeeIdAsync();
        var workTask = await AsyncExecuter.FirstOrDefaultAsync(
            (await WorkTaskRepository.GetQueryableAsync())
            .Where(x => x.Id == workTaskId)
            .Select(x => new
            {
                x.Id,
                x.BusinessCode,
                x.BusinessKey,
                x.AssigneeId,
                x.ReporterId
            }));
        if (workTask == null)
        {
            throw new Volo.Abp.UserFriendlyException("Không tìm thấy công việc.");
        }

        ValidateAccess(workTask.BusinessCode, workTask.AssigneeId, workTask.ReporterId, currentEmployeeId);

        var claimFolder = await AsyncExecuter.FirstOrDefaultAsync(
            (await ClaimFolderRepository.GetQueryableAsync())
            .Where(x => x.Id == workTask.BusinessKey)
            .Select(x => new
            {
                x.Id
            }));
        if (claimFolder == null)
        {
            throw new Volo.Abp.UserFriendlyException("Không tìm thấy hồ sơ bồi thường.");
        }

        var evaluateItems = await AsyncExecuter.ToListAsync(
            (await ResClaimEvaluateItemRepository.GetQueryableAsync())
            .OrderBy(x => x.Name)
            .Select(x => new
            {
                x.Id,
                x.Name
            }));

        var evaluation = await AsyncExecuter.FirstOrDefaultAsync(
            (await ClaimFolderEvaluateRepository.GetQueryableAsync())
            .Where(x => x.ClaimFolderId == claimFolder.Id)
            .Select(x => new
            {
                x.Id,
                x.Result,
                x.Description
            }));
        var detailResults = new Dictionary<Guid, string>();
        if (evaluation != null)
        {
            var details = await AsyncExecuter.ToListAsync(
                (await ClaimFolderEvaluateDetailRepository.GetQueryableAsync())
                .Where(x => x.ClaimFolderEvaluateId == evaluation.Id)
                .Select(x => new
                {
                    x.EvaluateItemId,
                    x.Result
                }));
            detailResults = details.ToDictionary(x => x.EvaluateItemId, x => x.Result);
        }

        return new DetailedAssessmentEvaluationDetailDto
        {
            WorkTaskId = workTask.Id,
            ClaimFolderId = claimFolder.Id,
            Result = evaluation?.Result,
            Description = evaluation?.Description,
            Items = evaluateItems.Select(x => new DetailedAssessmentEvaluationItemDto
            {
                EvaluateItemId = x.Id,
                Name = x.Name,
                Result = detailResults.GetValueOrDefault(x.Id)
            }).ToList()
        };
    }

    public virtual async Task SaveAsync(Guid workTaskId, SaveDetailedAssessmentEvaluationInput input)
    {
        var currentEmployeeId = await GetCurrentEmployeeIdAsync();
        var workTask = await AsyncExecuter.FirstOrDefaultAsync(
            (await WorkTaskRepository.GetQueryableAsync())
            .Where(x => x.Id == workTaskId)
            .Select(x => new
            {
                x.Id,
                x.BusinessCode,
                x.BusinessKey,
                x.AssigneeId,
                x.ReporterId
            }));
        if (workTask == null)
        {
            throw new Volo.Abp.UserFriendlyException("Không tìm thấy công việc.");
        }

        ValidateAccess(workTask.BusinessCode, workTask.AssigneeId, workTask.ReporterId, currentEmployeeId);

        var claimFolder = await AsyncExecuter.FirstOrDefaultAsync(
            (await ClaimFolderRepository.GetQueryableAsync())
            .Where(x => x.Id == workTask.BusinessKey)
            .Select(x => new
            {
                x.Id
            }));
        if (claimFolder == null)
        {
            throw new Volo.Abp.UserFriendlyException("Không tìm thấy hồ sơ bồi thường.");
        }

        await ValidateInputAsync(input);
        var normalizedResult = NormalizeYn(input.Result)!;
        var normalizedDescription = NormalizeDescription(input.Description);

        var evaluationInfo = await AsyncExecuter.FirstOrDefaultAsync(
            (await ClaimFolderEvaluateRepository.GetQueryableAsync())
            .Where(x => x.ClaimFolderId == claimFolder.Id)
            .Select(x => new
            {
                x.Id
            }));
        Guid evaluationId;
        if (evaluationInfo == null)
        {
            var evaluation = new ClaimFolderEvaluate(
                GuidGenerator.Create(),
                claimFolder.Id,
                currentEmployeeId,
                normalizedResult,
                normalizedDescription);
            await ClaimFolderEvaluateRepository.InsertAsync(evaluation, autoSave: true);
            evaluationId = evaluation.Id;
        }
        else
        {
            var currentUserId = CurrentUser.Id;
            await (await ClaimFolderEvaluateRepository.GetQueryableAsync())
                .Where(x => x.Id == evaluationInfo.Id)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.EmployeeId, currentEmployeeId)
                    .SetProperty(x => x.Result, normalizedResult)
                    .SetProperty(x => x.Description, normalizedDescription)
                    .SetProperty(x => x.LastModificationTime, _ => DateTime.Now)
                    .SetProperty(x => x.LastModifierId, currentUserId));
            evaluationId = evaluationInfo.Id;
        }

        await (await ClaimFolderEvaluateDetailRepository.GetQueryableAsync())
            .Where(x => x.ClaimFolderEvaluateId == evaluationId)
            .ExecuteDeleteAsync();

        foreach (var item in input.Items)
        {
            var detail = new ClaimFolderEvaluateDetail(
                GuidGenerator.Create(),
                evaluationId,
                item.EvaluateItemId,
                item.Result,
                null);
            await ClaimFolderEvaluateDetailRepository.InsertAsync(detail, autoSave: true);
        }
    }

    private async Task ValidateInputAsync(SaveDetailedAssessmentEvaluationInput input)
    {
        var result = NormalizeYn(input.Result);
        if (result == null)
        {
            throw new Volo.Abp.UserFriendlyException("Kết luận là bắt buộc.");
        }

        var evaluateItems = await AsyncExecuter.ToListAsync(
            (await ResClaimEvaluateItemRepository.GetQueryableAsync())
            .Select(x => new
            {
                x.Id
            }));
        var itemIds = evaluateItems.Select(x => x.Id).OrderBy(x => x).ToList();
        var inputIds = (input.Items ?? new List<SaveDetailedAssessmentEvaluationItemInput>())
            .Select(x => x.EvaluateItemId)
            .Distinct()
            .OrderBy(x => x)
            .ToList();

        if (itemIds.Count != inputIds.Count || !itemIds.SequenceEqual(inputIds))
        {
            throw new Volo.Abp.UserFriendlyException("Vui lòng đánh giá đầy đủ tất cả các hạng mục.");
        }

        foreach (var item in input.Items ?? Enumerable.Empty<SaveDetailedAssessmentEvaluationItemInput>())
        {
            if (NormalizeYn(item.Result) == null)
            {
                throw new Volo.Abp.UserFriendlyException("Mỗi hạng mục đánh giá phải chọn Đúng hoặc Không đúng.");
            }
        }
    }

    private void ValidateAccess(string businessCode, Guid? assigneeId, Guid reporterId, Guid currentEmployeeId)
    {
        if (businessCode != "CLAIM_DETAIL_ASSESSMENT")
        {
            throw new Volo.Abp.UserFriendlyException(L["ClaimTask:NotAuthorized"].Value);
        }

        if (assigneeId != currentEmployeeId && reporterId != currentEmployeeId)
        {
            throw new Volo.Abp.UserFriendlyException(L["ClaimTask:NotAuthorized"].Value);
        }
    }

    private static string? NormalizeYn(string? value)
    {
        var normalized = value?.Trim().ToUpperInvariant();
        return normalized is "Y" or "N" ? normalized : null;
    }

    private static string? NormalizeDescription(string? value)
    {
        var normalized = value?.Trim();
        return string.IsNullOrEmpty(normalized) ? null : normalized;
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
            .Select(x => new
            {
                x.Id
            }));
        if (employee == null)
        {
            throw new Volo.Abp.UserFriendlyException(L["ClaimTask:NotAuthorized"].Value);
        }

        return employee.Id;
    }
}
