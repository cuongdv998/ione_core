using System.Text.Json.Serialization;

namespace iOne.Workflow;

/// <summary>
/// Input for resuming/triggering an approval workflow step.
/// Serializes to JSON: { "action": "approve" } or { "action": "reject" }.
/// </summary>
public class WorkflowResumeInput
{
    [JsonPropertyName("action")]
    public string? Action { get; set; }
}
