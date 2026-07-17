using System;
using Volo.Abp.Application.Services;

namespace iOne.Master.ResUserDevices;

public interface IResUserDeviceAppService : ICrudAppService<
    ResUserDeviceDto,
    Guid,
    GetResUserDevicesInput,
    CreateResUserDeviceDto,
    UpdateResUserDeviceDto>
{
}
