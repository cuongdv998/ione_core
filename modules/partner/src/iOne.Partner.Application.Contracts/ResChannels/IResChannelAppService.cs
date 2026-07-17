using System;
using Volo.Abp.Application.Services;

namespace iOne.Partner.ResChannels;

public interface IResChannelAppService : ICrudAppService<
    ResChannelDto,
    Guid,
    GetResChannelsInput,
    CreateResChannelDto,
    UpdateResChannelDto>
{
}

