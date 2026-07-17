using System;
using System.Linq;
using System.Threading.Tasks;
using iOne.BusinessFlows;
using iOne.Workflow.WorkInstances;
using iOne.WorkInstances;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace iOne.Workflow.WorkInstances;

[RemoteService(Name = WorkflowRemoteServiceConsts.RemoteServiceName)]
public class WorkInstanceAppService : ApplicationService, IWorkInstanceAppService
{
    protected IRepository<BusinessFlow, Guid> BusinessFlowRepository { get; }
    protected IWorkInstanceRepository WorkInstanceRepository { get; }
    protected WorkInstanceManager WorkInstanceManager { get; }

    public WorkInstanceAppService(
        IRepository<BusinessFlow, Guid> businessFlowRepository,
        IWorkInstanceRepository workInstanceRepository,
        WorkInstanceManager workInstanceManager)
    {
        BusinessFlowRepository = businessFlowRepository;
        WorkInstanceRepository = workInstanceRepository;
        WorkInstanceManager = workInstanceManager;
    }

    public virtual async Task UpdateWorkInstanceStatusAsync(UpdateWorkInstanceStatusInput input)
    {
        if (string.IsNullOrWhiteSpace(input.WorkInstanceId))
            throw new UserFriendlyException("WorkInstanceId is required.");

        if (string.IsNullOrWhiteSpace(input.Status))
            throw new UserFriendlyException("Status is required.");

        if (!Enum.TryParse<WorkInstanceStatus>(input.Status.Trim(), ignoreCase: true, out var status))
            throw new UserFriendlyException($"Invalid status '{input.Status}'. Valid values: {string.Join(", ", Enum.GetNames<WorkInstanceStatus>())}.");

        var workInstanceQuery = await WorkInstanceRepository.GetQueryableAsync();
        var workInstance = await AsyncExecuter.FirstOrDefaultAsync(
            workInstanceQuery.Where(x => x.WorkflowInstanceId == input.WorkInstanceId.Trim()));
        if (workInstance == null)
            throw new UserFriendlyException($"Work instance with workflow instance id '{input.WorkInstanceId.Trim()}' not found.");

        await WorkInstanceManager.UpdateStatusAsync(workInstance, status);
    }

    public virtual async Task InitInstanceAsync(InitWorkInstanceDto input)
    {
        var businessFlow = await BusinessFlowRepository.FirstOrDefaultAsync(
            x => x.BusinessCode == input.BusinessFlow && !x.IsDeleted);

        if (businessFlow == null)
        {
            throw new UserFriendlyException($"BusinessFlow with code '{input.BusinessFlow}' not found.");
        }

        var workInstance = new WorkInstance(
            GuidGenerator.Create(),
            input.WorkflowInstanceId,
            businessFlow.Id,
            input.BusinessCode,
            input.BusinessName,
            input.BusinessKey,
            Clock.Now,
            null,
            WorkInstanceStatus.New);

        await WorkInstanceManager.CreateAsync(workInstance);
    }
}
