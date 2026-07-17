using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.DependencyInjection;

namespace iOne.Workflow.WorkflowFinish;

public class WorkflowFinishAppService : ApplicationService, IWorkflowFinishAppService, ITransientDependency
{
    private readonly IEnumerable<IWorkflowFinishHandler> _handlers;

    public WorkflowFinishAppService(IEnumerable<IWorkflowFinishHandler> handlers)
    {
        _handlers = handlers;
    }

    public async Task FinishAsync(WorkflowFinishInput input, CancellationToken cancellationToken = default)
    {
        if (input == null || string.IsNullOrWhiteSpace(input.BusinessName))
            return;

        var handler = _handlers.FirstOrDefault(h => h.CanHandle(input.BusinessName));
        if (handler == null)
            return;

        await handler.HandleAsync(input, cancellationToken);
    }
}
