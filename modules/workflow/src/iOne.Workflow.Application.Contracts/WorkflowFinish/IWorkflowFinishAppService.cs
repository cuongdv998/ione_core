using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace iOne.Workflow.WorkflowFinish;

public interface IWorkflowFinishAppService : IApplicationService
{
    Task FinishAsync(WorkflowFinishInput input, CancellationToken cancellationToken = default);
}
