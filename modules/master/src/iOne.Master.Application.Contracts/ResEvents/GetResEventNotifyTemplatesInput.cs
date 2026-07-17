using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace iOne.Master.ResEvents;

public class GetResEventNotifyTemplatesInput : PagedAndSortedResultRequestDto
{
    [Required]
    public Guid EventId { get; set; }
}

