using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace iOne.Workflow.WorkTasks;

public interface IWorkTaskInitAppService : IApplicationService
{
    /// <summary>
    /// Creates a work_task from Elsa workflow task data, resolves assignee from ResBusinessAssignee,
    /// and sets StartDate/EndDate/ActualStartDate from SLA.
    /// </summary>
    Task<InitWorkTaskResultDto> InitTaskAsync(InitWorkTaskDto input);
}
