using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace iOne.Workflow;

/// <summary>
/// Client for calling the Elsa workflow orchestrator API.
/// </summary>
public interface IElsaWorkflowService
{
    /// <summary>
    /// Initiates the policy termination workflow for the given policy version.
    /// Uses the current request's bearer token and passes the policy version id as BusinessKey.
    /// Workflow input includes terminationDate and totalRefundAmount for the workflow to use.
    /// </summary>
    /// <param name="policyVersionId">The policy version id (used as BusinessKey).</param>
    /// <param name="terminationDate">The termination date to pass to the workflow.</param>
    /// <param name="totalRefundAmount">The total refund amount to pass to the workflow.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task InitTerminatePolicyRequestAsync(Guid policyVersionId, DateTime terminationDate, decimal totalRefundAmount, CancellationToken cancellationToken = default);

    /// <summary>
    /// Initiates the create policy workflow for the given policy version.
    /// Uses the current request's bearer token; passes policy version id as businessKey, current user as reporterId, assigneeId and approverId.
    /// When <paramref name="useMotorbikeCreatePolicyWorkflow"/> is true, dispatches <see cref="ElsaWorkflowOptions.CreatePolicyMotorbikeWorkflowId"/> (must be configured).
    /// When <paramref name="approvalBusinessCode"/> is set, it is sent as workflow input <c>approvalBusinessCode</c> for Elsa init-task payloads.
    /// </summary>
    /// <param name="policyVersionId">The policy version id (used as businessKey).</param>
    /// <param name="approverId">Optional HrEmployee id for approver; empty string if not specified.</param>
    /// <param name="useMotorbikeCreatePolicyWorkflow">True to use the motorbike create-policy workflow definition id from configuration.</param>
    /// <param name="approvalBusinessCode">Optional work-task business code for Elsa (e.g. motorbike approval code).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task InitCreatePolicyWorkflowAsync(Guid policyVersionId, string? approverId = null, bool useMotorbikeCreatePolicyWorkflow = false, string? approvalBusinessCode = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Initiates the endorsement workflow for the given policy version.
    /// Uses the same input as create policy: current request's bearer token; policy version id as businessKey, current user as reporterId, assigneeId.
    /// </summary>
    /// <param name="policyVersionId">The policy version id (used as businessKey).</param>
    /// <param name="approverId">Optional approver id; empty string if not specified.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task InitEndorsementWorkflowAsync(Guid policyVersionId, string? approverId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Initiates the claim assignment workflow for the given claim.
    /// Uses the current request's bearer token; passes claim id as businessKey, current user as reporterId, assigneeId.
    /// </summary>
    /// <param name="claimId">The claim id (used as businessKey).</param>
    /// <param name="assigneeId">Optional assignee id (process employee); empty string if not specified.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task InitCreateClaimWorkflowAsync(Guid claimId, string? assigneeId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Initiates the claim folder workflow for the given claim folder.
    /// Uses the current request's bearer token; passes claim folder id as businessKey, current user as reporterId, assigneeId.
    /// </summary>
    /// <param name="claimFolderId">The claim folder id (used as businessKey).</param>
    /// <param name="assigneeId">Optional assignee id (process employee); empty string if not specified.</param>
    /// <param name="assigneeOrganizationId">Optional department/unit id for the assessor (workflow input).</param>
    /// <param name="assessmentStartDate">Optional planned assessment start (workflow input).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task InitCreateClaimFolderWorkflowAsync(
        Guid claimFolderId,
        string? assigneeId = null,
        Guid? assigneeOrganizationId = null,
        DateTime? assessmentStartDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Initiates the repair plan / PASC approval workflow.
    /// Uses the current request's bearer token; passes the given id as businessKey (e.g. ClaimFolderQuotationApproval id), current user as reporterId, assigneeId (approver).
    /// </summary>
    /// <param name="businessKey">Entity id passed to Elsa as businessKey (e.g. quotation approval row id).</param>
    /// <param name="assigneeId">HrEmployee id of the selected approver; empty string if not specified.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task InitCreateRepairPlanWorkflowAsync(Guid businessKey, string? assigneeId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Initiates the onsite assessment workflow for the given claim.
    /// Uses the current request's bearer token; passes claim id as businessKey, assignee and organization info.
    /// </summary>
    /// <param name="claimId">The claim id (used as businessKey).</param>
    /// <param name="assigneeOrganizationId">Department/unit ID for the onsite assessment.</param>
    /// <param name="assigneeId">Employee ID of the onsite assessor.</param>
    /// <param name="startDate">Planned start date of the assessment.</param>
    /// <param name="endDate">Planned end date of the assessment.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task InitOnsiteAssessmentWorkflowAsync(Guid claimId, Guid assigneeOrganizationId, Guid assigneeId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Triggers the approval workflow for the given instance with the specified action.
    /// Uses the Elsa Workflow API (event signaling) on the configured remote server: POST /events/{eventName}/trigger with input { "action": "approve" } or { "action": "reject" }.
    /// </summary>
    /// <param name="eventName">The event name (unique id from init-task) used in the trigger URL.</param>
    /// <param name="workflowInstanceId">The Elsa workflow instance id sent in the request body for routing.</param>
    /// <param name="action">The action: "approve" or "reject".</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task TriggerApprovalAsync(string eventName, string workflowInstanceId, string action, string? assigneeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Initiates the send notification workflow for the given notification IDs.
    /// Uses the current request's bearer token; passes notification IDs to the workflow for processing.
    /// </summary>
    /// <param name="notifyIds">List of SystemEventNotify IDs to be processed by the workflow.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task InitSendNotifyWorkflowAsync(List<Guid> notifyIds, CancellationToken cancellationToken = default);
}
