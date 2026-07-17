using System.Text.Json.Serialization;

namespace iOne.Workflow.WorkflowFinish;

public class WorkflowFinishInput
{
    [JsonPropertyName("workflowInstanceId")]
    public string? WorkflowInstanceId { get; set; }

    [JsonPropertyName("businessFlow")]
    public string? BusinessFlow { get; set; }

    [JsonPropertyName("businessCode")]
    public string? BusinessCode { get; set; }

    [JsonPropertyName("businessName")]
    public string? BusinessName { get; set; }

    [JsonPropertyName("businessKey")]
    public string? BusinessKey { get; set; }

    [JsonPropertyName("action")]
    public string? Action { get; set; }
}
