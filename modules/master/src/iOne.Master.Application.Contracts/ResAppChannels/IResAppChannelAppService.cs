using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace iOne.Master.ResAppChannels;

public interface IResAppChannelAppService : ICrudAppService<
    ResAppChannelDto,
    Guid,
    GetResAppChannelsInput,
    CreateResAppChannelDto,
    UpdateResAppChannelDto>
{
}
