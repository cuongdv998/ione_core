using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;
using iOne.Workflow.WorkflowFinish;
using iOne.Workflow.WorkInstances;
using iOne.Workflow.WorkTasks;

namespace iOne.Workflow.Controllers;

[RemoteService(Name = WorkflowRemoteServiceConsts.RemoteServiceName)]
[Area(WorkflowRemoteServiceConsts.ModuleName)]
[Route("api/workflow")]
public class WorkflowController : AbpControllerBase
{
    protected IWorkInstanceAppService WorkInstanceAppService { get; }
    protected IWorkTaskInitAppService WorkTaskInitAppService { get; }
    protected IWorkflowFinishAppService WorkflowFinishAppService { get; }
    protected IElsaWorkflowService ElsaWorkflowService { get; }

    public WorkflowController(
        IWorkInstanceAppService workInstanceAppService,
        IWorkTaskInitAppService workTaskInitAppService,
        IWorkflowFinishAppService workflowFinishAppService,
        IElsaWorkflowService elsaWorkflowService)
    {
        WorkInstanceAppService = workInstanceAppService;
        WorkTaskInitAppService = workTaskInitAppService;
        WorkflowFinishAppService = workflowFinishAppService;
        ElsaWorkflowService = elsaWorkflowService;
    }

    [HttpPost("init-instance")]
    public virtual Task InitInstanceAsync(InitWorkInstanceDto input)
    {
        return WorkInstanceAppService.InitInstanceAsync(input);
    }

    [HttpPost("init-task")]
    public virtual async Task<InitWorkTaskResultDto> InitTaskAsync(InitWorkTaskDto input)
    {
        return await WorkTaskInitAppService.InitTaskAsync(input);
    }

    [HttpPost("finish")]
    public virtual Task FinishAsync(WorkflowFinishInput input)
    {
        return WorkflowFinishAppService.FinishAsync(input);
    }

    [HttpPost("update-work-instance-status")]
    public virtual Task UpdateWorkInstanceStatusAsync(UpdateWorkInstanceStatusInput input)
    {
        return WorkInstanceAppService.UpdateWorkInstanceStatusAsync(input);
    }

    [HttpPost("send-notify")]
    public virtual Task SendNotifyAsync([FromBody] List<Guid> notifyIds)
    {
        return ElsaWorkflowService.InitSendNotifyWorkflowAsync(notifyIds);
    }
}
