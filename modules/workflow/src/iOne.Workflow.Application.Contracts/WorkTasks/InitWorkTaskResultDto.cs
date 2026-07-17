using System;

namespace iOne.Workflow.WorkTasks;

public class InitWorkTaskResultDto
{
    public Guid WorkTaskId { get; set; }

    public string EventName { get; set; } = null!;
}
