using iOne.ResAppChannels;
using Volo.Abp.Application.Dtos;

namespace iOne.Master.ResAppChannels;

public class GetResAppChannelsInput : PagedAndSortedResultRequestDto
{
    public string? Code { get; set; }
    
    public string? Name { get; set; }

    public ResAppChannelStatus? Status { get; set; }

    public ResAppChannelType? Type { get; set; }
}
