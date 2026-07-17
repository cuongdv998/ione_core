using System;
using Volo.Abp.Application.Dtos;

namespace iOne.Product.ProTableRateLines;

public class GetProTableRateLinesInput : PagedAndSortedResultRequestDto
{
    public Guid? TableRateId { get; set; }

    public Guid? CoverageId { get; set; }

    public Guid? ChannelId { get; set; }

    public Guid? PartnerId { get; set; }

    public string? Name { get; set; }
}
