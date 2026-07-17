using System;
using System.Collections.Generic;

namespace iOne.Master.SystemEventNotifies
{
    public class EventNotifyInfoDto
    {
        public string EventCode { get; set; } = default!;
        public Dictionary<string, object> Parameters { get; set; } = new();
        public Recipient Recipients { get; set; } = new();
        public DateTime Schedule { get; set; }
    }

    public class Recipient
    {
        public List<Guid> EmployeeIds { get; set; }
        public List<Guid> CustomerIds { get; set; }
    }
}
