using System;
using System.ComponentModel.DataAnnotations;
using iOne.SystemEventNotifies;
using Volo.Abp.Application.Dtos;

namespace iOne.Claim.Claims;

public class ClaimSentMessageDto : EntityDto<Guid>
{
    public DateTime CreationTime { get; set; }

    public DateTime? SentAt { get; set; }

    public string Recipient { get; set; } = null!;

    public string? ChannelName { get; set; }

    public string Title { get; set; } = null!;

    public string Body { get; set; } = null!;

    public SystemEventNotifyStatus Status { get; set; }
}

public class GetClaimSentMessagesInput : PagedAndSortedResultRequestDto
{
    [Required]
    public Guid ClaimId { get; set; }
}

