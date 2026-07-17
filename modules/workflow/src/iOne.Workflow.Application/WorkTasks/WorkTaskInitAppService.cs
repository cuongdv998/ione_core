using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using iOne.HrEmployees;
using iOne.ResBusinessAssignees;
using iOne.Workflow;
using iOne.ResTaskCategories;
using iOne.Workflow.WorkTasks;
using iOne.WorkInstances;
using iOne.WorkTasks;
using Volo.Abp;
using Volo.Abp.Application.Services;

namespace iOne.Workflow.WorkTasks;

[RemoteService(Name = WorkflowRemoteServiceConsts.RemoteServiceName)]
public class WorkTaskInitAppService : ApplicationService, IWorkTaskInitAppService
{
    protected IWorkInstanceRepository WorkInstanceRepository { get; }
    protected IResBusinessAssigneeRepository ResBusinessAssigneeRepository { get; }
    protected IResTaskCategoryRepository ResTaskCategoryRepository { get; }
    protected IHrEmployeeRepository HrEmployeeRepository { get; }
    protected WorkTaskManager WorkTaskManager { get; }
    protected IServiceScopeFactory ServiceScopeFactory { get; }
    protected IOptions<ElsaWorkflowOptions> ElsaWorkflowOptions { get; }

    public WorkTaskInitAppService(
        IWorkInstanceRepository workInstanceRepository,
        IResBusinessAssigneeRepository resBusinessAssigneeRepository,
        IResTaskCategoryRepository resTaskCategoryRepository,
        IHrEmployeeRepository hrEmployeeRepository,
        WorkTaskManager workTaskManager,
        IServiceScopeFactory serviceScopeFactory,
        IOptions<ElsaWorkflowOptions> elsaWorkflowOptions)
    {
        WorkInstanceRepository = workInstanceRepository;
        ResBusinessAssigneeRepository = resBusinessAssigneeRepository;
        ResTaskCategoryRepository = resTaskCategoryRepository;
        HrEmployeeRepository = hrEmployeeRepository;
        WorkTaskManager = workTaskManager;
        ServiceScopeFactory = serviceScopeFactory;
        ElsaWorkflowOptions = elsaWorkflowOptions;
    }

    public virtual async Task<InitWorkTaskResultDto> InitTaskAsync(InitWorkTaskDto input)
    {
        // Resolve work_instance by workflow_instance_id (string); work_task.work_instance_id = work_instance.id
        var workInstanceQuery = await WorkInstanceRepository.GetQueryableAsync();
        var workInstance = await AsyncExecuter.FirstOrDefaultAsync(
            workInstanceQuery.Where(x => x.WorkflowInstanceId == input.WorkflowInstanceId));
        if (workInstance == null)
        {
            throw new UserFriendlyException($"Work instance with workflow instance id '{input.WorkflowInstanceId}' not found.");
        }

        var taskCategoryQuery = await ResTaskCategoryRepository.GetQueryableAsync();
        var taskCategory = await AsyncExecuter.FirstOrDefaultAsync(
            taskCategoryQuery.Where(x => x.Code == input.TaskCategory && !x.IsDeleted));
        if (taskCategory == null)
        {
            throw new UserFriendlyException($"Task category with code '{input.TaskCategory}' not found.");
        }

        Guid? assigneeId = null;
        Guid? assigneeDepartmentId = null;
        Guid? assigneeOrganizationId = null;
        Guid? resBusinessAssigneeId = null;
        var scheduleSystemAutoApprove = false;

        if (!string.IsNullOrWhiteSpace(input.AssigneeId))
        {
            if (!Guid.TryParse(input.AssigneeId, out var parsed))
            {
                throw new UserFriendlyException($"Invalid assigneeId: '{input.AssigneeId}' is not a valid Guid.");
            }
            assigneeId = parsed;
        }
        else if (!string.IsNullOrWhiteSpace(input.BusinessAuthorityCode))
        {
            var now = Clock.Now;
            var assigneeQuery = await ResBusinessAssigneeRepository.GetQueryableAsync();
            var assignees = await AsyncExecuter.ToListAsync(
                assigneeQuery
                    .Where(x =>
                        x.BusinessCode == input.BusinessCode
                        && x.AuthorityCode == input.BusinessAuthorityCode
                        && x.EffectDate <= now
                        && (x.ExpireDate == null || x.ExpireDate >= now)
                        && x.Status == ResBusinessAssigneeStatus.Active
                        && !x.IsDeleted));

            var empRecords = assignees.Where(x => x.AssigneeType == ResBusinessAssigneeType.Emp).ToList();
            var roleRecords = assignees.Where(x => x.AssigneeType == ResBusinessAssigneeType.Role).ToList();
            var systemRecords = assignees.Where(x => x.AssigneeType == ResBusinessAssigneeType.System).ToList();

            if (empRecords.Count > 0)
            {
                var chosen = empRecords[Random.Shared.Next(empRecords.Count)];
                assigneeId = chosen.AssigneeId;
                assigneeDepartmentId = chosen.DepartmentId;
                assigneeOrganizationId = chosen.OrganizationId;
                resBusinessAssigneeId = chosen.Id;
            }
            else if (roleRecords.Count > 0)
            {
                var roleWithOrgAndRole = roleRecords
                    .Where(x => x.OrganizationId.HasValue && !string.IsNullOrWhiteSpace(x.AssigneeRole))
                    .ToList();
                if (roleWithOrgAndRole.Count > 0)
                {
                    var chosenRecord = roleWithOrgAndRole[Random.Shared.Next(roleWithOrgAndRole.Count)];
                    var employeeIds = await HrEmployeeRepository.GetIdsByOrgIdAndRoleCodeAsync(
                        chosenRecord.OrganizationId!.Value, chosenRecord.AssigneeRole!, CancellationToken.None);
                    if (employeeIds.Count > 0)
                    {
                        assigneeId = employeeIds[Random.Shared.Next(employeeIds.Count)];
                    }
                    assigneeDepartmentId = chosenRecord.DepartmentId;
                    assigneeOrganizationId = chosenRecord.OrganizationId;
                    resBusinessAssigneeId = chosenRecord.Id;
                }
                else
                {
                    var chosen = roleRecords[Random.Shared.Next(roleRecords.Count)];
                    assigneeDepartmentId = chosen.DepartmentId;
                    assigneeOrganizationId = chosen.OrganizationId;
                    resBusinessAssigneeId = chosen.Id;
                }
            }
            else if (systemRecords.Count > 0)
            {
                scheduleSystemAutoApprove = true;
                var chosen = systemRecords[Random.Shared.Next(systemRecords.Count)];
                assigneeDepartmentId = chosen.DepartmentId;
                assigneeOrganizationId = chosen.OrganizationId;
                resBusinessAssigneeId = chosen.Id;
            }
        }

        var startDate = Clock.Now;
        var endDate = startDate.Add(ParseSlaToTimeSpan(input.Sla));

        if (!Enum.TryParse<WorkTaskStatus>(input.Status, ignoreCase: true, out var status))
        {
            throw new UserFriendlyException($"Invalid task status: '{input.Status}'.");
        }

        if (!Enum.TryParse<WorkTaskPriority>(input.Priority, ignoreCase: true, out var priority))
        {
            throw new UserFriendlyException($"Invalid task priority: '{input.Priority}'.");
        }

        var workTaskCode = GenerateUniqueId();
        var eventName = GenerateUniqueId();
        var reporterId = Guid.Empty;
        if (Guid.TryParse(input.ReporterId, out var parsed_report))
        {
            reporterId = parsed_report;
        }
        reporterId = await GetCurrentEmployeeIdAsync(reporterId);
        var workTask = new WorkTask(
            GuidGenerator.Create(),
            workInstance.Id,
            input.BusinessCode,
            input.BusinessName,
            input.BusinessKey,
            formKey: null,
            workTaskCode,
            eventName,
            input.Name,
            description: null,
            reporterId,
            input.BusinessAuthorityCode,
            assigneeId,
            assigneeDepartmentId,
            assigneeOrganizationId,
            resBusinessAssigneeId,
            status,
            priority,
            taskCategory.Id,
            startDate,
            endDate,
            actualStartDate: startDate,
            actualEndDate: null,
            reasonId: null,
            reasonDescription: null);

        await WorkTaskManager.CreateAsync(workTask);

        if (scheduleSystemAutoApprove)
        {
            ScheduleSystemAssigneeAutoApprove(eventName, input.WorkflowInstanceId, workTask.Id);
        }

        return new InitWorkTaskResultDto { WorkTaskId = workTask.Id, EventName = eventName };
    }

    private async Task<Guid> GetCurrentEmployeeIdAsync(Guid? report_id)
    {
        if (CurrentUser.Id == null)
        {
            throw new UserFriendlyException("User is not authenticated.");
        }

        var query = await HrEmployeeRepository.GetQueryableAsync();
        var employee = await AsyncExecuter.FirstOrDefaultAsync(
            query.Where(e => e.UserId == report_id && !e.IsDeleted));
        if (employee == null)
        {
            // throw new UserFriendlyException("No employee record found for the current user.");
            return Guid.Empty;
        }

        return employee.Id;
    }

    private void ScheduleSystemAssigneeAutoApprove(string eventName, string workflowInstanceId, Guid workTaskId)
    {
        var delaySeconds = ElsaWorkflowOptions.Value.SystemAssigneeAutoApproveDelaySeconds;
        var scopeFactory = ServiceScopeFactory;
        var logger = Logger;

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(delaySeconds));

                using (var scope = scopeFactory.CreateScope())
                {
                    var workTaskRepository = scope.ServiceProvider.GetRequiredService<IWorkTaskRepository>();
                    var workTaskManager = scope.ServiceProvider.GetRequiredService<WorkTaskManager>();
                    var workTask = await workTaskRepository.GetAsync(workTaskId);
                    await workTaskManager.UpdateStatusAsync(workTask, WorkTaskStatus.Approved);

                    var elsaWorkflowService = scope.ServiceProvider.GetRequiredService<IElsaWorkflowService>();
                    await elsaWorkflowService.TriggerApprovalAsync(eventName, workflowInstanceId, "approve", null);
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex,
                    "System assignee auto-approve failed. EventName={EventName}, WorkflowInstanceId={WorkflowInstanceId}",
                    eventName, workflowInstanceId);
            }
        });
    }

    private static string GenerateUniqueId() => Guid.NewGuid().ToString("N");

    /// <summary>
    /// Parses SLA string to TimeSpan. Format: integer + suffix (d=day, w=week, m=month).
    /// Examples: "1d", "1w", "2m". Default when null/empty/invalid: 7 days.
    /// </summary>
    private static TimeSpan ParseSlaToTimeSpan(string? sla)
    {
        if (string.IsNullOrWhiteSpace(sla))
        {
            return TimeSpan.FromDays(7);
        }

        var s = sla.Trim();
        if (s.Length < 2)
        {
            return TimeSpan.FromDays(7);
        }

        var suffix = s[^1];
        if (!int.TryParse(s.AsSpan(0, s.Length - 1), out var value) || value <= 0)
        {
            return TimeSpan.FromDays(7);
        }

        return suffix switch
        {
            'd' or 'D' => TimeSpan.FromDays(value),
            'w' or 'W' => TimeSpan.FromDays(value * 7),
            'm' or 'M' => TimeSpan.FromDays(value * 30),
            _ => TimeSpan.FromDays(7)
        };
    }
}
