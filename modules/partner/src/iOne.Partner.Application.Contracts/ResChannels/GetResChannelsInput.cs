using iOne.ResChannels;
using Volo.Abp.Application.Dtos;

namespace iOne.Partner.ResChannels;

public class GetResChannelsInput : PagedAndSortedResultRequestDto
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public ResChannelStatus? Status { get; set; }
}

