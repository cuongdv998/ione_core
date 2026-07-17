namespace iOne.Workflow;

/// <summary>
/// Options for the Elsa workflow orchestrator client.
/// </summary>
public class ElsaWorkflowOptions
{
    public const string SectionName = "Elsa";

    /// <summary>
    /// Base URL of the Elsa workflow API (e.g. http://elsa.vnexco.com).
    /// </summary>
    public string BaseUrl { get; set; } = "http://elsa.vnexco.com";

    /// <summary>
    /// HTTP timeout in seconds. Default 30.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Username for Elsa API authentication.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Password for Elsa API authentication.
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Workflow definition ID for terminate policy workflow.
    /// </summary>
    public string TerminatePolicyWorkflowId { get; set; } = string.Empty;

    /// <summary>
    /// Workflow definition ID for create policy workflow (triggered on save draft).
    /// </summary>
    public string CreatePolicyWorkflowId { get; set; } = string.Empty;

    /// <summary>
    /// Workflow definition ID for motorbike create-policy approval (optional; used when dispatching motorbike issuance).
    /// </summary>
    public string CreatePolicyMotorbikeWorkflowId { get; set; } = string.Empty;

    /// <summary>
    /// Workflow definition ID for claim assignment workflow (triggered on save and send assessment).
    /// </summary>
    public string CreateClaimWorkflowId { get; set; } = string.Empty;

    /// <summary>
    /// Workflow definition ID for claim folder workflow (triggered on create claim folder).
    /// </summary>
    public string CreateClaimFolderWorkflowId { get; set; } = string.Empty;

    /// <summary>
    /// Workflow definition ID for repair plan approval (triggered when submitting repair plan for approval).
    /// </summary>
    public string CreateRepairPlanWorkflowId { get; set; } = string.Empty;

    /// <summary>
    /// Workflow definition ID for onsite assessment workflow (triggered when assigning onsite assessment for a claim).
    /// </summary>
    public string OnsiteAssessmentWorkflowId { get; set; } = string.Empty;

    /// <summary>
    /// Workflow definition ID for endorsement workflow (triggered after endorsement).
    /// </summary>
    public string EndorsementWorkflowId { get; set; } = "ee00000000000002";

    /// <summary>
    /// Workflow definition ID for send notification workflow (triggered after creating SystemEventNotify records).
    /// </summary>
    public string SendNotifyWorkflowId { get; set; } = string.Empty;

    /// <summary>
    /// Delay in seconds before auto-calling the approval trigger when init-task assignee type is System. Default 5.
    /// </summary>
    public int SystemAssigneeAutoApproveDelaySeconds { get; set; } = 1;
}
