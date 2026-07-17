using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using iOne.Claim.Claims;
using iOne.ClaimStages;
using iOne.ClaimSlas;
using ClaimEntity = iOne.Claims.Claim;
using iOne.ClaimFolders;
using iOne.Claim.Permissions;
using iOne.HrEmployees;
using iOne.ResClaimStageTasks;
using iOne.WorkTasks;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;

namespace iOne.Claim.Claims;

[Authorize(ClaimPermissions.Default)]
public class ClaimStageProgressAppService : ApplicationService, IClaimStageProgressAppService
{
    protected IRepository<ClaimStage, Guid> ClaimStageRepository { get; }
    protected IRepository<ClaimFolder, Guid> ClaimFolderRepository { get; }
    protected IRepository<ClaimSla, Guid> ClaimSlaRepository { get; }
    protected IRepository<WorkTask, Guid> WorkTaskRepository { get; }
    protected IRepository<ClaimEntity, Guid> ClaimRepository { get; }
    protected IRepository<ResClaimStageTask> ResClaimStageTaskRepository { get; }
    protected IRepository<HrEmployee, Guid> EmployeeRepository { get; }

    public ClaimStageProgressAppService(
        IRepository<ClaimStage, Guid> claimStageRepository,
        IRepository<ClaimFolder, Guid> claimFolderRepository,
        IRepository<ClaimSla, Guid> claimSlaRepository,
        IRepository<WorkTask, Guid> workTaskRepository,
        IRepository<ClaimEntity, Guid> claimRepository,
        IRepository<ResClaimStageTask> resClaimStageTaskRepository,
        IRepository<HrEmployee, Guid> employeeRepository)
    {
        ClaimStageRepository = claimStageRepository;
        ClaimFolderRepository = claimFolderRepository;
        ClaimSlaRepository = claimSlaRepository;
        WorkTaskRepository = workTaskRepository;
        ClaimRepository = claimRepository;
        ResClaimStageTaskRepository = resClaimStageTaskRepository;
        EmployeeRepository = employeeRepository;
    }

    public virtual async Task<PagedResultDto<ClaimStageProgressDto>> GetListAsync(GetClaimStageProgressInput input)
    {
        if (input.ClaimId == Guid.Empty)
        {
            throw new UserFriendlyException(L["Claim:ClaimIdRequired"]);
        }

        var claim = await ClaimRepository.FindAsync(input.ClaimId);
        var insurerId = claim?.InsurerId;

        var query = await ClaimStageRepository.GetQueryableAsync();
        query = query
            .Include(x => x.ClaimFolder)
            .Include(x => x.Stage)
            .Where(x => x.ClaimId == input.ClaimId);

        if (input.ClaimFolderId.HasValue)
        {
            query = query.Where(x => x.ClaimFolderId == input.ClaimFolderId.Value);
        }

        var totalCount = await AsyncExecuter.CountAsync(query);

        var stages = await AsyncExecuter.ToListAsync(
            query
                .OrderByDescending(x => x.StartDate ?? x.DueDate)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount > 0 ? input.MaxResultCount : 20));

        var stageIds = stages.Select(x => x.StageId).Distinct().ToList();

        var now = Clock.Now;
        var slaQuery = await ClaimSlaRepository.GetQueryableAsync();
        slaQuery = slaQuery
            .Where(s => stageIds.Contains(s.ClaimStageId))
            .Where(s => s.Status == ClaimSlaStatus.Active)
            .Where(s => s.EffectDate <= now)
            .Where(s => s.ExpireDate == null || s.ExpireDate >= now);
        if (insurerId.HasValue)
        {
            slaQuery = slaQuery.Where(s => s.InsurerId == insurerId.Value);
        }
        var slaList = await AsyncExecuter.ToListAsync(slaQuery);
        var slaByStageId = slaList.GroupBy(s => s.ClaimStageId).ToDictionary(g => g.Key, g => g.First());

        var stageTaskQuery = await ResClaimStageTaskRepository.GetQueryableAsync();
        var stageTasks = await AsyncExecuter.ToListAsync(
            stageTaskQuery.Where(st => stageIds.Contains(st.ClaimStageId ?? Guid.Empty)));
        var taskCategoryIdsByStageId = stageTasks
            .Where(st => st.ClaimStageId.HasValue && st.TaskCategoryId.HasValue)
            .GroupBy(st => st.ClaimStageId!.Value)
            .ToDictionary(g => g.Key, g => g.Select(st => st.TaskCategoryId!.Value).Distinct().ToList());

        var workTaskQuery = await WorkTaskRepository.GetQueryableAsync();
        var workTasks = await AsyncExecuter.ToListAsync(
            workTaskQuery.Where(wt => wt.BusinessCode == "claim" && wt.BusinessKey == input.ClaimId));

        var assigneeIds = workTasks
            .Where(wt => wt.AssigneeId.HasValue)
            .Select(wt => wt.AssigneeId!.Value)
            .Distinct()
            .ToList();
        var employees = assigneeIds.Count > 0
            ? await AsyncExecuter.ToListAsync(
                (await EmployeeRepository.GetQueryableAsync()).Where(e => assigneeIds.Contains(e.Id)))
            : new List<HrEmployee>();
        var assigneeNameById = employees.ToDictionary(e => e.Id, e => e.FullName ?? e.Id.ToString());

        var dtos = new List<ClaimStageProgressDto>();
        foreach (var s in stages)
        {
            slaByStageId.TryGetValue(s.StageId, out var sla);

            WorkTask? workTaskForStage = null;
            if (taskCategoryIdsByStageId.TryGetValue(s.StageId, out var taskCategoryIds) && taskCategoryIds.Count > 0)
            {
                workTaskForStage = workTasks.FirstOrDefault(wt => taskCategoryIds.Contains(wt.TaskCategoryId));
            }
            workTaskForStage ??= workTasks.FirstOrDefault();

            double? actualMinutes = null;
            if (s.StartDate.HasValue)
            {
                var end = s.EndDate ?? Clock.Now;
                actualMinutes = (end - s.StartDate.Value).TotalMinutes;
            }

            string? slaText = null;
            if (sla != null && actualMinutes.HasValue)
            {
                var actualM = actualMinutes.Value;
                var slaM = (double)sla.SlaTime;
                if (actualM < 60 && slaM < 60)
                {
                    slaText = $"{(int)Math.Round(actualM)}p/{(int)slaM}p";
                }
                else
                {
                    slaText = $"{Math.Round(actualM / 60, 1)}h/{Math.Round(slaM / 60.0, 1)}h";
                }
            }

            string? performerName = null;
            if (workTaskForStage?.AssigneeId != null && assigneeNameById.TryGetValue(workTaskForStage.AssigneeId.Value, out var empName))
            {
                performerName = empName;
            }
            else
            {
                performerName = workTaskForStage?.AssigneeId?.ToString();
            }

            var dto = new ClaimStageProgressDto
            {
                Id = s.Id,
                ClaimStageId = s.Id,
                ClaimId = s.ClaimId,
                ClaimFolderId = s.ClaimFolderId,
                FolderNo = s.ClaimFolder?.FolderNo,
                StageName = s.Stage?.Name ?? s.StageId.ToString(),
                TaskName = workTaskForStage?.Name,
                PerformerName = performerName,
                StartDate = s.StartDate,
                DueDate = s.DueDate,
                EndDate = s.EndDate,
                SlaTime = sla?.SlaTime,
                ActualMinutes = actualMinutes,
                SlaText = slaText,
                Status = s.Status,
                WorkTaskStatus = workTaskForStage?.Status
            };

            dtos.Add(dto);
        }

        return new PagedResultDto<ClaimStageProgressDto>(totalCount, dtos);
    }
}

