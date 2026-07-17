using System.Threading;
using System.Threading.Tasks;

namespace iOne.Workflow.WorkflowFinish;

public interface IWorkflowFinishHandler
{
    bool CanHandle(string businessName);

    Task HandleAsync(WorkflowFinishInput input, CancellationToken cancellationToken = default);
}
