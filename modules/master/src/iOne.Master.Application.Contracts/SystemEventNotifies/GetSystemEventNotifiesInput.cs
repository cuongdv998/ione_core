using System;
using iOne.SystemEventNotifies;
using Volo.Abp.Application.Dtos;

namespace iOne.Master.SystemEventNotifies;

public class GetSystemEventNotifiesInput : PagedAndSortedResultRequestDto
{
    public string? EventCode { get; set; }
    public Guid? AppChannelId { get; set; }
    public string? RecipientType { get; set; }
    public Guid? RecipientId { get; set; }
    public string? Recipient { get; set; }
    public SystemEventNotifyStatus? Status { get; set; }
    public DateTime? ScheduleAtFrom { get; set; }
    public DateTime? ScheduleAtTo { get; set; }
    public DateTime? SentAtFrom { get; set; }
    public DateTime? SentAtTo { get; set; }
}
