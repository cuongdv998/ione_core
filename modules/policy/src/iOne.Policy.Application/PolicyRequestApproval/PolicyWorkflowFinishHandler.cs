using System;
using System.Threading;
using System.Threading.Tasks;
using iOne.Workflow.WorkflowFinish;
using Volo.Abp.DependencyInjection;

namespace iOne.Policy.PolicyRequestApproval;

/// <summary>
/// Policy workflow finish handler. Policy workflow completion is now split into three separate APIs.
/// The workflow engine (e.g. Elsa) should call in order: POST /policy/update-status (with policyVersionId), then
/// POST /policy/partner-create-policy (with policyVersionId, if action is approve), then POST /workflow/update-work-instance-status.
/// This handler no longer performs any operations.
/// </summary>
public class PolicyWorkflowFinishHandler : IWorkflowFinishHandler, ITransientDependency
{
    private const string PolicyBusinessName = "policyVersion";

    public bool CanHandle(string businessName)
    {
        return string.Equals(businessName, PolicyBusinessName, StringComparison.OrdinalIgnoreCase);
    }

    public Task HandleAsync(WorkflowFinishInput input, CancellationToken cancellationToken = default)
    {
        // No-op: policy workflow finish logic has been moved to separate APIs.
        // Call POST /policy/update-status, POST /policy/partner-create-policy (if approve), POST /workflow/update-work-instance-status.
        return Task.CompletedTask;
    }
}
